using Reposcraper.Extractors.Values;
using Reposcraper.Savers.Values;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reposcraper.Savers
{
    public static class AggregatedJsonSaver
    {
        public static async Task SaveAllResultsAsync(
            Dictionary<IExtractedData, List<ReadabilityScore>> scoreResults,
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

                SaveFileContent aggregatedData = new SaveFileContent
                (
                    timeStamp: DateTime.UtcNow.ToString("o"),
                    totalItems: scoreResults.Count,
                    results: scoreResults.Select(kvp => new SaveResultContent
                    (
                        code: ConvertToJsonObject(kvp.Key),
                        readabilityScores: kvp.Value
                    )).ToList()
                );

                string jsonString = JsonSerializer.Serialize(aggregatedData, SaverJsonContext.Default.SaveFileContent);
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
