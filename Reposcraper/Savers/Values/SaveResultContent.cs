using System.Text.Json.Serialization;

namespace Reposcraper.Savers.Values
{
    public class SaveResultContent
    {
        [JsonPropertyName("code")]
        public object Code { get; init; }

        [JsonPropertyName("readabilityScores")]
        public List<ReadabilityScore> ReadabilityScores { get; init; }

        public SaveResultContent(object code, List<ReadabilityScore> readabilityScores)
        {
            Code = code;
            ReadabilityScores = new List<ReadabilityScore>(readabilityScores);
        }
    }
}
