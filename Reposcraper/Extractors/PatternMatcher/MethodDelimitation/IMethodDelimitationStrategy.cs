using Reposcraper.Extractors.PatternMatcher;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors.PatternMatcher.MethodDelimitation
{
    public interface IMethodDelimitationStrategy
    {
        public string ExtractMethod(Match methodMatch, string[] lines, int startLineNumber, IPatternMatcher patternMatcher);
    }
}
