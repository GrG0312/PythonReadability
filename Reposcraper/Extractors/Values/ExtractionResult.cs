using Reposcraper.Scrapers.Values;

namespace Reposcraper.Extractors.Values
{
    public readonly struct ExtractionResult
    {
        public readonly RepositoryFile SourceFile;
        public readonly bool IsSuccessful;
        public readonly string? ErrorMessage;
        public readonly IReadOnlyList<IExtractedData> ExtractedData;

        public ExtractionResult(
            RepositoryFile sourceFile,
            bool isSuccessful,
            IEnumerable<IExtractedData> extractedData,
            string? errorMessage = null)
        {
            SourceFile = sourceFile;
            IsSuccessful = isSuccessful;
            ExtractedData = extractedData.ToList();
            ErrorMessage = errorMessage;
        }
    }
}
