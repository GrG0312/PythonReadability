namespace Reposcraper.Judger
{
    /// <summary>
    /// Represents a request to evaluate the readability of code content.
    /// </summary>
    public readonly struct ReadabilityEvaluationRequest
    {
        public readonly string? Language;
        public readonly string? Context;
        public readonly string Code;
        public readonly IReadOnlyList<string> Metrics;

        public ReadabilityEvaluationRequest(string? language, string? context, string code, IEnumerable<string> metrics)
        {
            Language = language;
            Context = context;
            Code = code;
            Metrics = [.. metrics];
        }
    }
}
