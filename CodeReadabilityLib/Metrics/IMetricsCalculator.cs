namespace CodeReadabilityLib.Metrics
{
    public interface IMetricsCalculator
    {
        public Task<IReadOnlyList<ReadabilityScore>> CalculateAsync(
            string code, SupportedLanguage language, IEnumerable<MetricDefinition> metrics, CancellationToken ctoken = default);
    }
}
