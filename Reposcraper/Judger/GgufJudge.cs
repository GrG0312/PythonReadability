using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Reposcraper.Judger
{
    /// <summary>
    /// GGUF readability judge that communicates with the model via llama.cpp server.
    /// </summary>
    public class GgufJudge : IReadabilityJudge, IDisposable
    {
        private readonly string _ggufFilePath;
        private readonly int _port;
        private readonly HttpClient _httpClient;
        private Process? _llamaServerProcess;
        private bool _isRunning;
        private readonly int _gpuLayers;

        public GgufJudge(string ggufFilePath, int port = 8080, int gpuLayers = 99)
        {
            _ggufFilePath = ggufFilePath;
            _port = port;
            _gpuLayers = gpuLayers;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{_port}"),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        #region Start - Stop Llama.cpp
        /// <summary>
        /// Starts the llama.cpp server with the GGUF model.
        /// </summary>
        public async Task StartLlamaAsync()
        {
            if (_isRunning) return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "llama-server", // TODO: where is the llama server
                Arguments = $"-m \"{_ggufFilePath}\" --port {_port} -ngl {_gpuLayers} --host 0.0.0.0",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            _llamaServerProcess = Process.Start(startInfo);

            if (_llamaServerProcess is null)
            {
                throw new InvalidOperationException("Failed to start llama-server process.");
            }

            await WaitForServerToBeReady();
            _isRunning = true;
        }

        /// <summary>
        /// Stops the llama.cpp server.
        /// </summary>
        public void StopLlama()
        {
            if (_llamaServerProcess is not null && !_llamaServerProcess.HasExited)
            {
                _llamaServerProcess.Kill(entireProcessTree: true);
                _llamaServerProcess.WaitForExit(5000);
                _llamaServerProcess.Dispose();
                _llamaServerProcess = null;
            }
            _isRunning = false;
        }

        private async Task WaitForServerToBeReady(int maxAttempts = 30, int millisecondsDelay = 1000)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync("/health");
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                } catch { /* Server not yet ready */ }
                await Task.Delay(millisecondsDelay);
            }

            throw new TimeoutException("GGUF server did not become ready in time.");
        }
        #endregion

        #region Evaluate request
        public async Task<IReadOnlyList<ReadabilityScore>> EvaluateAsync(ReadabilityEvaluationRequest request, CancellationToken ctoken = default)
        {
            if (!_isRunning) throw new InvalidOperationException("GGUF Judge must be started before evaluation.");

            string prompt = BuildPrompt(request);
            string response = await QueryLlmAsync(prompt, ctoken);

            return ParseReadabilityScore(response);
        }

        private string BuildPrompt(ReadabilityEvaluationRequest request)
        {
            StringBuilder sb = new StringBuilder();

            // TODO: revise prompt
            sb.AppendLine("You are a code readability expert. Analyze the following code and provide a readability score from 0 to 100.");
            sb.AppendLine("100 means extremely readable, 0 means completely unreadable.");
            sb.AppendLine("Consider factors like: naming conventions, code structure, comments, complexity, and clarity.");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(request.Language))
            {
                sb.AppendLine($"Language: {request.Language}");
            }

            if (!string.IsNullOrEmpty(request.Context))
            {
                sb.AppendLine($"Context: {request.Context}");
            }

            sb.AppendLine();
            sb.AppendLine("Code to analyze:");
            sb.AppendLine("```");
            sb.AppendLine(request.Code);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("Respond with ONLY a JSON object in this exact format:");
            sb.AppendLine("{\"readability_score\": <number between 1 and 100>}");

            return sb.ToString();
        }

        private async Task<string> QueryLlmAsync(string prompt, CancellationToken ctoken)
        {
            // TODO: revise request body and customize for LLM
            var requestBody = new
            {
                prompt,
                temperature = 0.1,
                max_tokens = 200,
                stop = new[] { "```", "\n\n\n" }
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/completion", content, ctoken);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync(ctoken);
            using JsonDocument doc = JsonDocument.Parse(responseJson);

            return doc.RootElement.GetProperty("content").GetString() ?? string.Empty;
        }

        private IReadOnlyList<ReadabilityScore> ParseReadabilityScore(string llmResponse)
        {
            // TODO: there must be a much simpler solution than this
            try
            {
                int jsonStart = llmResponse.IndexOf('{');
                int jsonEnd = llmResponse.IndexOf('}');

                if (jsonStart >= 0 && jsonEnd >= jsonStart)
                {
                    string jsonPart = llmResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    using JsonDocument doc = JsonDocument.Parse(jsonPart);

                    if (doc.RootElement.TryGetProperty("readability_score", out JsonElement scoreElement))
                    {
                        int score = scoreElement.GetInt32();
                        score = Math.Clamp(score, 0, 100);

                        // TODO: parse more readability scores
                        return new[] { new ReadabilityScore("LLM_Readability", score) };
                    }
                }
            }
            catch
            {
                // Fallback: try to find any number in the response
                Match match = Regex.Match(llmResponse, @"\b(\d{1,3})\b");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int score))
                {
                    score = Math.Clamp(score, 1, 100);
                    return new[] { new ReadabilityScore("LLM_Readability", score) };
                }
            }

            // Return a default value if the response is totally invalid
            return new[] { new ReadabilityScore("Fallback_readability", 50) };
        }
        #endregion

        public void Dispose()
        {
            StopLlama();
            _httpClient.Dispose();
        }
    }
}
