using CodeReadabilityLib.Extractors.PatternMatcher.MethodDelimitation;
using System.Text.RegularExpressions;

namespace CodeReadabilityLib.Extractors.PatternMatcher
{
    public interface IPatternMatcher
    {
        public Regex MethodRegex { get; }
        public Regex ClassRegex { get; }
        public Regex CommentRegex { get; }
        public IMethodDelimitationStrategy MethodDelimitationStrategy { get; }

        public bool CanHandle(string fileExtension);
    }
}
