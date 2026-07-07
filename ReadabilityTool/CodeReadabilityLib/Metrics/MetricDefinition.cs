using CodeReadabilityLib.Languages;

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
        public IReadOnlyList<ProgLang> ApplicableLanguages { get; init; } = [];
        public int? MinValue { get; init; }
        public int? MaxValue { get; init; }
        public bool HigherIsBetter { get; init; } = true;

        public MetricDefinition() { }
        public MetricDefinition(string id, string name, string description, IReadOnlyList<ProgLang> applicableLanguages, 
            int? minValue = null, int? maxValue = null, bool higherIsBetter = true)
        {
            Id = id;
            Name = name;
            Description = description;
            ApplicableLanguages = applicableLanguages;
            MinValue = minValue;
            MaxValue = maxValue;
            HigherIsBetter = higherIsBetter;
        }

        public bool AppliesTo(ProgLang language) =>
            ApplicableLanguages.Count == 0 || ApplicableLanguages.Contains(language);
    }

    public record AlgorithmicMetricDefinition : MetricDefinition
    {
        public Func<string, SupportedLanguage, int> Algorithm { get; init; }

        public AlgorithmicMetricDefinition()
        {
            Algorithm = (_, _) => 0; // Default algorithm that returns 0
        }
        public AlgorithmicMetricDefinition(string id, string name, string description, IReadOnlyList<ProgLang> applicableLanguages, 
            Func<string, SupportedLanguage, int> algorithm, int? minValue = null, int? maxValue = null, bool higherIsBetter = true)
            : base(id, name, description, applicableLanguages, minValue, maxValue, higherIsBetter)
        {
            Algorithm = algorithm;
        }
    }
}
