using Reposcraper.Extractors.Values;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reposcraper.Savers
{
    public static class AggregatedJsonSaver
    {
        public static async Task SaveAllResultsAsync(
            List<(IExtractedData data, ReadabilityScore score)> results,
            string outputPath)
        {
            try
            {
                // Ensure output directory exists
                string? directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var aggregatedData = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    totalItems = results.Count,
                    results = results.Select(r => new
                    {
                        code = ConvertToJsonObject(r.data),
                        readabilityScore = r.score.ScoreValue,
                        metric = r.score.MetricName
                    }).ToList()
                };

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                string jsonString = JsonSerializer.Serialize(aggregatedData, options);
                await File.WriteAllTextAsync(outputPath, jsonString);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to save aggregated results: {ex.Message}", ex);
            }
        }

        private static object ConvertToJsonObject(IExtractedData data)
        {
            return data switch
            {
                MethodData method => new
                {
                    type = "method",
                    signature = method.MethodSignature,
                    body = method.MethodBody,
                    comments = method.PrecedingComments,
                    sourceFile = new
                    {
                        name = method.SourceFile.Name,
                        path = method.SourceFile.Path,
                        repository = method.SourceFile.HomeRepo.RepoName,
                        owner = method.SourceFile.HomeRepo.Owner
                    }
                },
                _ => new
                {
                    type = "unknown",
                    data = data.ToString()
                }
            };
        }
    }
}
