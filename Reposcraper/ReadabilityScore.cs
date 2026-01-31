namespace Reposcraper
{
    /// <summary>
    /// Represents the readability score of a piece of content based on a specific metric.
    /// </summary>
    public readonly struct ReadabilityScore
    {
        /// <summary>
        /// The name of the readability metric.
        /// </summary>
        public readonly string MetricName;

        /// <summary>
        /// Represents the score value associated with this instance. Is between 0 and 100 (inclusive).
        /// </summary>
        public readonly int ScoreValue;
        public ReadabilityScore(string metricName, int scoreValue)
        {
            MetricName = metricName;
            ScoreValue = scoreValue;
        }
    }
}
