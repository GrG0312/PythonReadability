using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeReadabilityLib.Judger
{
    public class RangeGgufJudge : IDisposable
    {
        private readonly string _ggufPath;
        private readonly int _port;
        private readonly int _gpuLayers;
        private readonly int _minScore;
        private readonly int _maxScore;

        private readonly HttpClient _httpClient;
        private Process? _server;
        private bool _running;

        public RangeGgufJudge(string ggufPath, int port, int gpuLayers, int minScore, int maxScore)
        {
            _ggufPath = ggufPath;
            _port = port;
            _gpuLayers = gpuLayers;
            _minScore = minScore;
            _maxScore = maxScore;
            _httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{_port}"), Timeout = TimeSpan.FromMinutes(3) };
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
                FileName = "llama-server",
                Arguments = $"-m \"{_ggufPath}\" --port {_port} -ngl {_gpuLayers} --host 0.0.0.0",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _server = Process.Start(psi) ?? throw new InvalidOperationException("Could not start llama-server.");
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
            using HttpResponseMessage response = await _httpClient.PostAsync("/v1/chat/completions", content, ctoken);
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
        private async Task WaitUntilReadyAsync(int attempts = 30, int delayMs = 1000)
        {
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync("/health");
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
                _server.Dispose();
            }
            _httpClient.Dispose();
        }
    }
}
