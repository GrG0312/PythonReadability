namespace CodeReadabilityLib.Languages
{
    public record LanguageContext
    {
        public const int INDENT_SIZE = 4;
        public const int TABULATOR_SIZE = 4;

        public required bool UsesSignificantIndent { get; init; }

        public required IReadOnlySet<string> SingleLineCommentPrefixes { get; init; }
        public required IReadOnlySet<(string Start, string End)> MultiLineCommentDelimiters { get; init; }

        public required IReadOnlySet<char> BlockOpeningTokens { get; init; }
        public required IReadOnlySet<char> BlockClosingTokens { get; init; }

        public required IReadOnlySet<string> CyclomaticKeywords { get; init; }
        public required IReadOnlySet<string> Operators { get; init; }

        public string[] SplitCode(string code)
        {
            return
                code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        public int CountLeadingWhitespaces(string line)
        {
            int i = 0;
            while (i < line.Length)
            {
                if (line[i] is ' ')
                {
                    i++;
                }
                else if (line[i] is '\t')
                {
                    i += TABULATOR_SIZE;
                }
            }

            return i;
        }
    }
}
