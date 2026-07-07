using CodeReadabilityLib.Extractors.PatternMatcher;
using CodeReadabilityLib.Extractors.Values;
using CodeReadabilityLib.Scrapers.Values;

namespace CodeReadabilityLib.Extractors
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
