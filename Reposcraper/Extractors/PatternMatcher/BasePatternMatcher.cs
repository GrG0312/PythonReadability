using Reposcraper.Extractors.PatternMatcher.MethodDelimitation;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors.PatternMatcher
{
    public abstract class BasePatternMatcher : IPatternMatcher
    {
        public Regex MethodRegex { get; }
        public Regex ClassRegex { get; }
        public Regex CommentRegex { get; }
        public IMethodDelimitationStrategy MethodDelimitationStrategy { get; }

        protected BasePatternMatcher(
            Regex methodRegex,
            Regex classRegex,
            Regex commentRegex,
            IMethodDelimitationStrategy mds)
        {
            MethodRegex = methodRegex;
            ClassRegex = classRegex;
            CommentRegex = commentRegex;
            MethodDelimitationStrategy = mds;
        } 

        public abstract bool CanHandle(string fileExtension);
    }
}
