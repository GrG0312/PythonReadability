using System.Text.Json.Serialization;

namespace CodeReadabilityLib.Evaluation
{
    /// <summary>
    /// Schema for the concrete answer section of the evaluation report, representing the model's specific answer for a given code snippet and its original score.
    /// </summary>
    public sealed class ConcreteAnswer
    {
        [JsonPropertyName("snippetId")]
        public string SnippetId { get; set; } = string.Empty;

        [JsonPropertyName("originalScore")]
        public double OriginalScore { get; set; }

        [JsonPropertyName("modelAnswer")]
        public double ModelAnswer { get; set; }
    }
}
