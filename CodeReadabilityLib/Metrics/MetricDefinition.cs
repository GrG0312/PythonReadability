namespace CodeReadabilityLib.Metrics
{
    /// <summary>
    /// Represents a single readability metric with its metadata.
    /// </summary>
    public record MetricDefinition
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public IReadOnlyList<SupportedLanguage> ApplicableLanguages { get; init; } = [];
        public double? MinValue { get; init; }
        public double? MaxValue { get; init; }
        public bool HigherIsBetter { get; init; } = true;

        public bool AppliesTo(SupportedLanguage language) =>
            ApplicableLanguages.Count == 0 || ApplicableLanguages.Contains(language);
    }
}
