using System.Text.Json.Serialization;

namespace CodeReadabilityLib.Evaluation
{
    /// <summary>
    /// Schema for each model's results in the evaluation report.
    /// </summary>
    public sealed record ModelResult
    {
        [JsonPropertyName("modelName")]
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Number of exact/missed answers.
        /// Indexes mean how much the model missed the answer by:
        /// 0: exact answer, 
        /// 1: missed by 1, 
        /// 2: missed by 2, 
        /// 3: missed by 3 or more.
        /// </summary>
        [JsonPropertyName("answers")]
        public int[] Answers { get; set; } = new int[4];

        [JsonPropertyName("concreteAnswers")]
        public List<ConcreteAnswer> ConcreteAnswers { get; set; } = new List<ConcreteAnswer>();
    }
}
