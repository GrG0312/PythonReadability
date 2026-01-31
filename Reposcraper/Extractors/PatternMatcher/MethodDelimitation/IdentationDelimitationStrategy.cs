using Reposcraper.Extractors.PatternMatcher;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors.PatternMatcher.MethodDelimitation
{
    /// <summary>
    /// Method delimitation strategy based on indentation levels.
    /// </summary>
    public class IndentationDelimitationStrategy : IMethodDelimitationStrategy
    {
        /// <summary>
        /// Number of spaces a tab character represents.
        /// </summary>
        public int TabSize { get; }

        public IndentationDelimitationStrategy(int tabSize = 4)
        {
            TabSize = tabSize;
        }

        /// <summary>
        /// Extracts the source code lines corresponding to a method definition from the provided lines, starting at the
        /// specified line number.
        /// </summary>
        /// <remarks>The extraction is based on indentation levels, assuming that lines indented beyond
        /// the method signature belong to the method body. Empty or whitespace-only lines within the method are
        /// preserved in the output.</remarks>
        /// <param name="methodMatch">A regular expression match object representing the method signature to extract.</param>
        /// <param name="lines">An array of source code lines from which the method will be extracted.</param>
        /// <param name="startLineNumber">The 0-based line number indicating where the method signature begins in the lines array.</param>
        /// <param name="patternMatcher">An object used to match patterns within the source code lines, which may assist in identifying method
        /// boundaries.</param>
        /// <returns>A string containing the extracted lines of the method body (excluding the signature line).
        /// Returns an empty string if the start line is outside the bounds of the lines array.</returns>
        public string ExtractMethod(Match methodMatch, string[] lines, int startLineNumber, IPatternMatcher patternMatcher)
        {
            int currentLine = startLineNumber;
            List<string> methodLines = new List<string>();

            if (currentLine >= lines.Length)
                return string.Empty;

            // Determine base indentation level from the signature line, but do NOT include the signature itself
            int baseIndentation = GetIndentationLevel(lines[currentLine]);
            currentLine++;

            // Continue while we have lines that are more indented than the base level
            // or are empty/whitespace-only lines
            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];

                // Handle empty or whitespace-only lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    methodLines.Add(line);
                    currentLine++;
                    continue;
                }

                int lineIndentation = GetIndentationLevel(line);

                // If we encounter a line with equal or less indentation than the method signature,
                // and it's not empty, we've reached the end of the method
                if (lineIndentation <= baseIndentation)
                {
                    break;
                }

                methodLines.Add(line);
                currentLine++;
            }

            return string.Join('\n', methodLines);
        }

        /// <summary>
        /// Gets the indentation level of a line (number of leading whitespace characters).
        /// </summary>
        private int GetIndentationLevel(string line)
        {
            int indentation = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                    indentation++;
                else if (c == '\t')
                    indentation += TabSize;
                else
                    break;
            }
            return indentation;
        }
    }
}
