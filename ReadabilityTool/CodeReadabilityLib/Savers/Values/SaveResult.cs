using CodeReadabilityLib.Extractors.Values;

namespace CodeReadabilityLib.Savers.Values
{
    public readonly struct SaveResult
    {
        public readonly bool IsSuccessful;
        public readonly string? ErrorMessage;
        public readonly string OutputPath;
        public readonly IExtractedData Data;

        public SaveResult(bool isSuccessful, string? errorMessage, string outputPath, IExtractedData data)
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
            OutputPath = outputPath;
            Data = data;
        }
    }
}
