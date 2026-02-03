using System.Text.Json.Serialization;

namespace Reposcraper.Savers.Values
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(SaveFileContent))]
    [JsonSerializable(typeof(SaveResultContent))]
    [JsonSerializable(typeof(List<SaveResultContent>))]
    public partial class SaverJsonContext : JsonSerializerContext { }
}
