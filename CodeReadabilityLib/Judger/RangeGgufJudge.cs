using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeReadabilityLib.Judger
{
    public class RangeGgufJudge : IDisposable
    {
        private readonly string _ggufPath;
        private readonly string _llamaServerPath;
        private readonly int _port;
        private readonly int _gpuLayers;
        private readonly int _minScore;
        private readonly int _maxScore;

        private readonly HttpClient _httpClient;
        private Process? _server;
        private bool _running;

        public RangeGgufJudge(string ggufPath, string llamaServerPath, int port, int gpuLayers, int minScore, int maxScore)
        {
            _ggufPath = ggufPath;
            _llamaServerPath = llamaServerPath;
            _port = port;
            _gpuLayers = gpuLayers;
            _minScore = minScore;
            _maxScore = maxScore;
            _httpClient = new HttpClient 
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        /// <summary>
        /// Starts the llama-server process with the specified GGUF model and waits until it's ready to accept requests.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If a server could not be started.</exception>
        public async Task StartAsync()
        {
            if (_running) return;

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = _llamaServerPath,
                Arguments = $"-m \"{_ggufPath}\" --port {_port} -ngl {_gpuLayers} --host 0.0.0.0 --ctx-size 4096 --parallel 1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Explicitly forward GPU environment to child process
            string? cudaDevices = Environment.GetEnvironmentVariable("CUDA_VISIBLE_DEVICES");
            if (!string.IsNullOrEmpty(cudaDevices))
            {
                psi.Environment["CUDA_VISIBLE_DEVICES"] = cudaDevices;
                Console.WriteLine($"Forwarding CUDA_VISIBLE_DEVICES={cudaDevices} to llama-server");
            }
            else
            {
                Console.WriteLine("WARNING: CUDA_VISIBLE_DEVICES not set in environment!");
            }

            _server = Process.Start(psi) ?? throw new InvalidOperationException("Could not start llama-server.");

            // Write llama-server output to a dedicated log file
            string llamaLog = $"/output/llama-server-{Path.GetFileNameWithoutExtension(_ggufPath)}.log";
            StreamWriter logWriter = new StreamWriter(llamaLog, append: false) { AutoFlush = true };

            _server.OutputDataReceived += (s, e) => { if (e.Data != null) logWriter.WriteLine(e.Data); };
            _server.ErrorDataReceived += (s, e) => { if (e.Data != null) logWriter.WriteLine($"ERR: {e.Data}"); };
            _server.BeginOutputReadLine();
            _server.BeginErrorReadLine();

            await WaitUntilReadyAsync();
            _running = true;
        }

        public async Task<int> ScoreSnippetAsync(string code, CancellationToken ctoken = default)
        {
            if (!_running) throw new InvalidOperationException("Judge must be started before scoring.");

            string systemPrompt =
                $"You are a strict code readability rater. Return ONLY compact JSON: {{\"score\": int}}. " +
                $"The score must be an integer from {_minScore} to {_maxScore}. " +
                $"{_minScore} = very poor readability, {_maxScore} = excellent readability.";

            string userPrompt =
                "Score the following code snippet for readability.\n\n" +
                "Code:\n-----BEGIN-CODE-----\n" + code + "\n-----END-CODE-----\n\n" +
                "Return only JSON with the 'score' field.";

            var payload = new
            {
                messages = new[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1,
                max_tokens = 100
            };

            using StringContent content = new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ctoken);
            cts.CancelAfter(TimeSpan.FromMinutes(5));

            using HttpResponseMessage response = await _httpClient.PostAsync($"http://localhost:{_port}/v1/chat/completions", content, cts.Token);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(ctoken);
            int score = ParseScore(ExtractContent(json));
            return Math.Clamp(score, _minScore, _maxScore);
        }

        private static string ExtractContent(string responseJson)
        {
            using JsonDocument doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        private static int ParseScore(string text)
        {
            Match match = Regex.Match(text.Trim(), @"\b(\d+)\b"); // extract first digit
            if (match.Success) return int.Parse(match.Groups[1].Value);
            return -1; // Fallback error value handled by clamping
        }

        /// <summary>
        /// Polls the /health endpoint until the server is ready or times out after a certain number of attempts.
        /// </summary>
        /// <param name="attempts">How many tries we allow to reach the server.</param>
        /// <param name="delayMs">The delay between each try.</param>
        /// <exception cref="TimeoutException">If the connection could not be established in time.</exception>
        private async Task WaitUntilReadyAsync(int attempts = 360, int delayMs = 5000)
        {
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync($"http://localhost:{_port}/health");
                    if (response.IsSuccessStatusCode) return;
                }
                catch { /* wait */ }
                await Task.Delay(delayMs);
            }
            throw new TimeoutException("llama-server did not become ready in time.");
        }

        public void Dispose()
        {
            if (_server is not null && !_server.HasExited)
            {
                _server.Kill(entireProcessTree: true);
                _server.WaitForExit(10000);
                _server.Dispose();
            }
            _httpClient.Dispose();
        }
    }
}
