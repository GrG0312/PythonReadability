using Reposcraper.Extractors.PatternMatcher.MethodDelimitation;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors.PatternMatcher
{
    public class PythonPatternMatcher : BasePatternMatcher
    {
        public PythonPatternMatcher() : base(
            methodRegex: new Regex(@"^\s*def\s+\w+\s*\([^)]*\)\s*:", RegexOptions.Multiline | RegexOptions.Compiled),
            classRegex: new Regex(@"^\s*class\s+\w+.*:", RegexOptions.Multiline | RegexOptions.Compiled),
            commentRegex: new Regex(@"^\s*(#|""\""|''')", RegexOptions.Multiline | RegexOptions.Compiled),
            mds: new IndentationDelimitationStrategy(tabSize: 4))
        {
        }

        public override bool CanHandle(string fileExtension)
        {
            return fileExtension.Equals(".py", StringComparison.OrdinalIgnoreCase);
        }
    }
}
