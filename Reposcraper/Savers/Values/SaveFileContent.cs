using System.Text.Json.Serialization;

namespace Reposcraper.Savers.Values
{
    public class SaveFileContent
    {
        [JsonPropertyName("timeStamp")]
        public string TimeStamp { get; init; }

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; init; }

        [JsonPropertyName("results")]
        public List<SaveResultContent> Results { get; init; }

        public SaveFileContent(string timeStamp, int totalItems, List<SaveResultContent> results)
        {
            TimeStamp = timeStamp;
            TotalItems = totalItems;
            Results = results;
        }
    }
}
