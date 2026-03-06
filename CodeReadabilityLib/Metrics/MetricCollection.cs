namespace CodeReadabilityLib.Metrics
{
    public sealed class MetricCollection
    {
        public required IReadOnlyList<MetricDefinition> Universal { get; init; }
        public required IReadOnlyList<MetricDefinition> LanguageSpecific { get; init; }

        /// <summary>
        /// Gets all metrics in this collection.
        /// </summary>
        public IEnumerable<MetricDefinition> All 
        {
            get 
            {
                return Universal.Concat(LanguageSpecific);
            }
        }

        /// <summary>
        /// Gets all metrics applicable to a given language.
        /// </summary>
        public IEnumerable<MetricDefinition> GetForLanguage(SupportedLanguage language)
        {
            return Universal.Concat(LanguageSpecific.Where(m => m.AppliesTo(language)));
        }
    }
}
