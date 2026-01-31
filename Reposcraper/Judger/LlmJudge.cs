using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reposcraper.Judger
{
    /// <summary>
    /// Base class for LLM-based readability judges.
    /// </summary>
    public abstract class LlmJudge : IReadabilityJudge
    {
        /// <summary>
        /// Default JSON serialization options.
        /// </summary>
        protected static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Represents the result returned by the LLM judge.
        /// </summary>
        protected sealed class JudgeResult
        {
            /// <summary>
            /// List of individual score items.
            /// </summary>
            [JsonPropertyName("scores")]
            public List<JudgeScoreItem>? Scores { get; set; }
        }

        /// <summary>
        /// Represents an individual score item in the judge result.
        /// </summary>
        protected sealed class JudgeScoreItem
        {
            /// <summary>
            /// Gets or sets the name of the metric being scored.
            /// </summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the numerical score associated with the object.
            /// </summary>
            [JsonPropertyName("score")]
            public int Score { get; set; }
        }

        /// <summary>
        /// Issues a completion request to the underlying LLM.
        /// </summary>
        /// <remarks>
        /// Implementations must return raw text as produced by the model; higher-level parsing is handled by this base class.
        /// </remarks>
        /// <param name="systemPromt">The system prompt that constrains the assistant behavior and output schema.</param>
        /// <param name="userPrompt">The user prompt containing the evaluation request, metrics, and code.</param>
        /// <param name="ctoken">A cancellation token used to cancel the operation.</param>
        /// <returns>
        /// A task that resolves to the raw string response returned by the LLM.
        /// </returns>

        protected abstract Task<string> CompleteAsync(string systemPromt, string userPrompt, CancellationToken ctoken = default);

        /// <summary>
        /// Evaluates the readability of the provided code across the requested metrics using an LLM.
        /// </summary>
        /// <remarks>
        /// This method builds deterministic system and user prompts, invokes <see cref="CompleteAsync(string, string, CancellationToken)"/>,
        /// then extracts and deserializes the JSON payload into strongly typed results.
        /// </remarks>
        /// <param name="request">The evaluation request containing the code, language/context hints, and the set of metrics to score.</param>
        /// <param name="ctoken">A cancellation token used to cancel the evaluation.</param>
        /// <returns>
        /// A read-only list of <see cref="ReadabilityScore"/> where each item corresponds to a requested metric.
        /// Missing metrics in the LLM response are added with a score of <c>0</c>. All scores are clamped to the range [0, 100].
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/>.<c>Metrics</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when no metrics are specified in <paramref name="request"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the LLM returns an empty response.</exception>
        public async Task<IReadOnlyList<ReadabilityScore>> EvaluateAsync(ReadabilityEvaluationRequest request, CancellationToken ctoken = default)
        {
            if (request.Metrics is null)
            {
                throw new ArgumentNullException(nameof(request.Metrics));
            }
            if (request.Metrics.Count == 0)
            {
                throw new ArgumentException("At least one metric must be specified.", nameof(request.Metrics));
            }

            // Build prompts
            string systemPromt = BuildSystemPromt();
            string userPrompt = BuildUserPrompt(request);

            // Get raw response from LLM
            string rawResponse = await CompleteAsync(systemPromt, userPrompt, ctoken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                throw new InvalidOperationException("The LLM returned an empty response.");
            }

            // Extract and parse JSON
            string json = ExtractJsonObject(rawResponse);
            JudgeResult? parsed = ParseJudgeResult(json);

            List<ReadabilityScore> scores = new(parsed?.Scores?.Count ?? 0);
            if (parsed?.Scores != null)
            {
                foreach (JudgeScoreItem item in parsed.Scores)
                {
                    string name = (item.Name ?? string.Empty).Trim();
                    if(name.Length == 0)
                    {
                        continue;
                    }
                    scores.Add(new ReadabilityScore(item.Name, Clamp0To100(item.Score)));
                }
            }

            // Ensure all requested metrics are present
            HashSet<string> present = new HashSet<string>(scores.Select(s => s.MetricName), StringComparer.OrdinalIgnoreCase);
            foreach (string metric in request.Metrics)
            {
                if (!present.Contains(metric))
                {
                    scores.Add(new ReadabilityScore(metric, 0));
                }
            }

            return scores;
        }

        /// <summary>
        /// Builds the system prompt that constrains the LLM's behavior and output schema.
        /// </summary>
        /// <returns>
        /// The system prompt string.
        /// </returns>
        protected virtual string BuildSystemPromt()
        {
            return "You are a strict, deterministic code readability evaluator. " +
                   "Return ONLY compact JSON with this exact schema and no extra text: " +
                   "{\"scores\":[{\"name\":string,\"score\":int}]} " +
                   "Rules: 'score' must be an integer between 0 and 100 inclusive. " +
                   "Do not include explanations, markdown, code fences, or any fields beyond 'scores'.";
        }

        /// <summary>
        /// Builds a prompt string instructing an evaluator to assess the readability of the provided code sample across
        /// specified metrics.
        /// </summary>
        /// <remarks>The returned prompt enforces a strict response format, requiring only JSON output
        /// with metric scores and no additional explanation or formatting. Override this method to customize the prompt
        /// structure or instructions for different evaluation scenarios.</remarks>
        /// <param name="request">The request containing the code to evaluate, the list of metrics to score, and optional language and context
        /// information. Cannot be null.</param>
        /// <returns>A formatted string containing instructions, metrics, and the code sample, suitable for use as a prompt in
        /// readability evaluation workflows.</returns>
        protected virtual string BuildUserPrompt(ReadabilityEvaluationRequest request)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Evaluate the following code for readability across the listed metrics.");
            if (!string.IsNullOrWhiteSpace(request.Language))
            {
                sb.AppendLine("Language: " + request.Language!.Trim());
            }
            if (!string.IsNullOrWhiteSpace(request.Context))
            {
                sb.AppendLine("Language: " + request.Context!.Trim());
            }

            sb.AppendLine();
            sb.AppendLine("Metrics to score (0-100 integer for each):");
            foreach (string metric in request.Metrics)
            {
                sb.AppendLine("- " + metric);
            }

            sb.AppendLine();
            sb.AppendLine("Code:");
            sb.AppendLine("-----BEGIN-CODE-----");
            sb.AppendLine(request.Code);
            sb.AppendLine("-----END-CODE-----");

            sb.AppendLine();
            sb.AppendLine("Return ONLY JSON with the schema: {\"scores\":[{\"name\":string,\"score\":int}]}");
            sb.AppendLine("No markdown. No explanations. No extra keys.");
            return sb.ToString();
        }

        /// <summary>
        /// Extracts the JSON object from the raw LLM response.
        /// </summary>
        /// <param name="rawResponse">
        /// The raw response string from the LLM, which may contain additional text or formatting.
        /// </param>
        /// <returns>
        /// A string containing only the JSON object extracted from the response.
        /// </returns>
        protected virtual string ExtractJsonObject(string rawResponse)
        {
            string trimmed = rawResponse.Trim();

            // Remove ``` fences if present
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                int firstNewline = trimmed.IndexOf('\n');
                if (firstNewline >= 0)
                {
                    trimmed = trimmed[(firstNewline + 1)..];
                    int lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
                    if (lastFence >= 0)
                        trimmed = trimmed[..lastFence];
                }
            }

            int start = trimmed.IndexOf('{');
            int end = trimmed.LastIndexOf('}');
            if (start >= 0 && end >= start)
            {
                return trimmed[start..(end + 1)];
            }

            // Let the caller see the bad payload to ease troubleshooting
            return trimmed;
        }

        /// <summary>
        /// Parses the JSON string into a <see cref="JudgeResult"/> object.
        /// </summary>
        /// <param name="json">
        /// The JSON string to parse.
        /// </param>
        /// <returns>
        /// A <see cref="JudgeResult"/> object if parsing is successful; otherwise, <c>null</c>.
        /// </returns>
        protected virtual JudgeResult? ParseJudgeResult(string json)
        {
            return JsonSerializer.Deserialize<JudgeResult>(json, DefaultJsonOptions);
        }

        /// <summary>
        /// Restricts an integer value to the inclusive range of 0 to 100.
        /// </summary>
        /// <param name="score">The value to clamp. Values less than 0 are set to 0; values greater than 100 are set to 100.</param>
        /// <returns>An integer between 0 and 100, inclusive. Returns 0 if the input is less than 0, 100 if greater than 100, or
        /// the original value if within range.</returns>
        protected static int Clamp0To100(int score)
        {
            if (score < 0) return 0;
            if (score > 100) return 100;
            return score;
        }
    }
}
