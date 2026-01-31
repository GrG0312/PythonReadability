using Reposcraper.Extractors.PatternMatcher.MethodDelimitation;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors.PatternMatcher
{
    public class CSharpPatternMatcher : BasePatternMatcher
    {
        public CSharpPatternMatcher() : base(
            methodRegex: new Regex(@"^\s*(?:public|private|protected|internal|static|virtual|override|abstract|sealed|async|extern|unsafe|\w+\s+)*\w+(?:<[^>]*>)?\s+\w+\s*\([^)]*\)(?:\s*:\s*\w+(?:\([^)]*\))?)?(?=\s*(?:\{|=>))", RegexOptions.Multiline | RegexOptions.Compiled),
            classRegex: new Regex(@"^\s*(?:public|private|protected|internal)?\s*(?:abstract|sealed|static|partial)?\s*(?:class|struct|interface|record|enum)\s+\w+(?:<[^>]*>)?(?:\s*:\s*[^{]+)?(?=\s*\{)", RegexOptions.Multiline | RegexOptions.Compiled),
            commentRegex: new Regex(@"^\s*(//|/\*|\*/|\*)", RegexOptions.Multiline | RegexOptions.Compiled),
            mds: new BraceDelimitationStrategy("{", "}"))
        { }

        public override bool CanHandle(string fileExtension)
        {
            return fileExtension.Equals(".cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
