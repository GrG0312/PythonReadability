using CodeReadabilityLib.Languages;

namespace CodeReadabilityLib.Metrics
{
    public class MetricsCalculator : IMetricsCalculator
    {
        public Task<IReadOnlyList<ReadabilityScore>> CalculateAsync(string code, ProgLang language,
            IEnumerable<MetricDefinition> metrics, CancellationToken ctoken = default)
        {
            List<ReadabilityScore> scores = new List<ReadabilityScore>();

            foreach (MetricDefinition metric in metrics)
            {
                if (ctoken.IsCancellationRequested) break;
                if (!metric.AppliesTo(language)) continue;

                double? rawValue = CalculateRawValue(metric.Id, code);

                if (rawValue.HasValue)
                {
                    int clamped = Math.Clamp((int)Math.Round(rawValue.Value), 0, 100);
                    scores.Add(new ReadabilityScore(metric.Id, clamped));
                }
            }

            return Task.FromResult((IReadOnlyList<ReadabilityScore>)scores);
        }

        private double? CalculateRawValue(string metricId, string code)
        {
            return metricId switch
            {
                "average_line_length" => CalculateAverageLineLength(code),
                "comment_density" => CalculateCommentDensity(code),
                "lines_of_code" => CalculateLinesOfCode(code),
                "nesting_depth" => CalculateNestingDepth(code),
                // TODO: implement remaining algorithmic metrics
                _ => null
            };
        }

        private static double CalculateAverageLineLength(string code)
        {
            string[] lines = code.Split('\n');

            if (lines.Length == 0)
            {
                return 0;
            }

            return lines.Average(line => line.TrimEnd().Length);
        }

        private static double CalculateCommentDensity(string code)
        {
            string[] lines = code.Split('\n');
            if (lines.Length == 0) return 0;

            int commentLines = lines.Count(l =>
            {
                string trimmed = l.Trim();
                // TODO: this is a very naive way to detect comments and should be improved to handle different languages
                return trimmed.StartsWith("//") || trimmed.StartsWith('#') || trimmed.StartsWith("/*") || trimmed.StartsWith('*');
            });

            return (double)commentLines / lines.Length * 100;
        }

        private static double CalculateLinesOfCode(string code)
        {
            string[] lines = code.Split('\n');
            return lines.Count(l => !string.IsNullOrWhiteSpace(l));
        }

        private static double CalculateNestingDepth(string code)
        {
            return 0.0; // TODO: implement a proper nesting depth calculation that can handle different languages and constructs
        }
    }
}
