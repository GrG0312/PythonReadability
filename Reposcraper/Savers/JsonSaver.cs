using Reposcraper.Extractors.Values;
using Reposcraper.Savers.Values;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reposcraper.Savers
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
                // Ensure output directory exists
                string? directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

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

        private object ConvertToJsonObject(IExtractedData data)
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
