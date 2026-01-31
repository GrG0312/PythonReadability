using Reposcraper.Extractors.PatternMatcher;
using Reposcraper.Extractors.Values;
using Reposcraper.Scrapers.Values;

namespace Reposcraper.Extractors
{
    public abstract class ExtractorBase : IDataExtractor
    {
        /// <summary>
        /// Pattern matcher to identify method signatures and related patterns.
        /// </summary>
        protected readonly IPatternMatcher _patternMatcher;

        public ExtractorBase(IPatternMatcher patternMatcher)
        {
            _patternMatcher = patternMatcher;
        }

        public abstract Task<ExtractionResult> ExtractAsync(RepositoryFile file);
    }
}
