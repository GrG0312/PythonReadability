using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeReadabilityLib.Judger
{
    /// <summary>
    /// GGUF readability judge that communicates with the model via llama.cpp server.
    /// </summary>
    public class GgufJudge : LlmJudge, IDisposable
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

        protected override async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ctoken = default)
        {
            if (!_isRunning)
            {
                throw new InvalidOperationException("GGUF Judge must be started before evaluation.");
            }

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1,
                max_tokens = 2000
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/v1/chat/completions", content, ctoken);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync(ctoken);
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        public void Dispose()
        {
            StopLlama();
            _httpClient.Dispose();
        }
    }
}
