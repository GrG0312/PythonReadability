using System.Text.Json.Serialization;

namespace CodeReadabilityLib.Evaluation
{
    /// <summary>
    /// The base schema for the evaluation report JSON file.
    /// </summary>
    public sealed class EvaluationReport
    {
        [JsonPropertyName("totalSnippets")]
        public int TotalSnippets { get; set; }

        [JsonPropertyName("modelResults")]
        public List<ModelResult> ModelResults { get; set; } = new List<ModelResult>();
    }
}
