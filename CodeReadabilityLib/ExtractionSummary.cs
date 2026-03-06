using CodeReadabilityLib.Scrapers.Values;

namespace CodeReadabilityLib
{
    public readonly struct ExtractionSummary
    {
        public readonly RepositoryInfo RepositoryInfo;
        public readonly uint TotalFiles;
        public readonly uint TotalExtractions;
        public readonly uint SuccessfulExtractions;

    }
}
