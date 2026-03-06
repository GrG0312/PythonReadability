using CodeReadabilityLib.Extractors.Values;
using CodeReadabilityLib.Savers.Values;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeReadabilityLib.Savers
{
    /// <summary>
    /// Saves readability evaluation results to JSON files.
    /// </summary>
    public class JsonDataSaver : IDataSaver
    {
        public async Task<SaveResult> SaveDataAsync(IExtractedData data, string outputPath)
        {
            try
            {
                EnsureDirectoryExists(outputPath);

                object jsonData = ConvertToJsonObject(data);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                string jsonString = JsonSerializer.Serialize(jsonData, options);
                await File.WriteAllTextAsync(outputPath, jsonString);

                return new SaveResult(
                    isSuccessful: true,
                    errorMessage: null,
                    outputPath: outputPath,
                    data: data);
            }
            catch (Exception ex)
            {
                return new SaveResult(
                    isSuccessful: false,
                    errorMessage: $"Failed to save data: {ex.Message}",
                    outputPath: outputPath,
                    data: data);
            }
        }

        public async Task SaveAllResultsAsync(
            Dictionary<IExtractedData, List<ReadabilityScore>> scoreResults,
            string outputPath)
        {
            try
            {
                EnsureDirectoryExists(outputPath);

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

        private static void EnsureDirectoryExists(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
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
