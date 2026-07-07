using CodeReadabilityLib.Extractors.PatternMatcher;
using System.Text.RegularExpressions;

namespace CodeReadabilityLib.Extractors.PatternMatcher.MethodDelimitation
{
    public interface IMethodDelimitationStrategy
    {
        public string ExtractMethod(Match methodMatch, string[] lines, int startLineNumber, IPatternMatcher patternMatcher);
    }
}
