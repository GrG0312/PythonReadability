using Reposcraper.Extractors.Values;
using Reposcraper.Scrapers.Values;

namespace Reposcraper.Extractors
{
    public interface IDataExtractor
    {
        /// <summary>
        /// Extracts data from a single repository file.
        /// The implementing class is responsible for fetching the file content.
        /// </summary>
        /// <param name="sourceFile">The repository file metadata.</param>
        /// <returns>The extraction result for the given file.</returns>
        public Task<ExtractionResult> ExtractAsync(RepositoryFile sourceFile);

        /// <summary>
        /// Extracts data from multiple repository files.
        /// The implementing class is responsible for fetching the contents of all files.
        /// </summary>
        /// <param name="sourceFiles">The repository files to extract data from.</param>
        /// <returns>A collection of extraction results, one per input file (for which extraction succeeded).</returns>
        public async Task<IReadOnlyCollection<ExtractionResult>> ExtractAsync(IEnumerable<RepositoryFile> sourceFiles)
        {
            List<ExtractionResult> results = new List<ExtractionResult>();
            foreach (RepositoryFile file in sourceFiles)
            {
                ExtractionResult result = await ExtractAsync(file);
                results.Add(result);
            }
            return results;
        }
    }
}
