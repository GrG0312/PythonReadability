using CodeReadabilityLib.Languages;

namespace CodeReadabilityLib.Metrics
{
    public interface IMetricsCalculator
    {
        public Task<IReadOnlyList<ReadabilityScore>> CalculateAsync(
            string code, ProgLang language, IEnumerable<MetricDefinition> metrics, CancellationToken ctoken = default);
    }
}
