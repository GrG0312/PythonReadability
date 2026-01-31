using Reposcraper.Extractors.PatternMatcher;
using System.Text;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors.PatternMatcher.MethodDelimitation
{
    /// <summary>
    /// A method delimitation strategy that uses braces to determine the start and end of methods.
    /// </summary>
    public class BraceDelimitationStrategy : IMethodDelimitationStrategy
    {
        /// <summary>
        /// The opening delimiters used to identify the start of a method body, stored in a string.
        /// </summary>
        public string OpeningDelimiters { get; }

        /// <summary>
        /// The closing delimiters used to mark the end of a section or token, stored in a string.
        /// </summary>
        public string ClosingDelimiters { get; }

        public BraceDelimitationStrategy(string openingDelimiters = "{", string closingDelimiters = "}")
        {
            OpeningDelimiters = openingDelimiters;
            ClosingDelimiters = closingDelimiters;
        }


        /// <summary>
        /// Extracts the method body from the provided lines starting from the method signature match.
        /// </summary>
        /// <param name="methodMatch">A regular expression match representing the method signature to extract. Must not be null.</param>
        /// <param name="lines">An array of strings containing the lines of source code to search. Must not be null.</param>
        /// <param name="startLineNumber">The zero-based index of the line in the source code where the method signature begins. Must be within the
        /// bounds of the lines array.</param>
        /// <param name="patternMatcher">An implementation of IPatternMatcher used to assist in pattern matching within the method extraction
        /// process. Must not be null.</param>
        /// <returns>A string containing only the method body, including the opening and closing braces.
        /// Returns an empty string if the method body cannot be fully extracted.</returns>
        public string ExtractMethod(Match methodMatch, string[] lines, int startLineNumber, IPatternMatcher patternMatcher)
        {
            // Find the line that contains the opening brace
            int braceStartLine = -1;
            int braceStartColumn = -1;

            // Start searching from the method signature line
            for (int lineIndex = startLineNumber; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                int braceIndex = line.IndexOf('{');

                if (braceIndex != -1)
                {
                    braceStartLine = lineIndex;
                    braceStartColumn = braceIndex;
                    break;
                }

                // Check for lambda arrow (expression-bodied member)
                int arrowIndex = line.IndexOf("=>");
                if (arrowIndex != -1)
                {
                    // For expression-bodied members, find the end (either semicolon or end of expression)
                    StringBuilder expressionBody = new StringBuilder();
                    bool foundEnd = false;

                    for (int i = lineIndex; i < lines.Length && !foundEnd; i++)
                    {
                        string currentLine = lines[i];
                        if (i == lineIndex)
                        {
                            // Start from the arrow position
                            expressionBody.AppendLine(currentLine.Substring(arrowIndex));
                        }
                        else
                        {
                            expressionBody.AppendLine(currentLine);
                        }

                        // Check if line ends with semicolon (end of expression-bodied member)
                        if (currentLine.TrimEnd().EndsWith(';'))
                        {
                            foundEnd = true;
                        }
                    }

                    return expressionBody.ToString().Trim();
                }
            }

            // If no opening brace found, return empty
            if (braceStartLine == -1)
            {
                return string.Empty;
            }

            // Count braces to find the matching closing brace
            int openBraces = 0;
            int closeBraceLine = -1;
            int closeBraceColumn = -1;
            bool foundClosing = false;

            for (int lineIndex = braceStartLine; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                int startCol = (lineIndex == braceStartLine) ? braceStartColumn : 0;

                for (int charIndex = startCol; charIndex < line.Length; charIndex++)
                {
                    char c = line[charIndex];

                    // Skip string literals and character literals
                    if (c == '"' || c == '\'')
                    {
                        char quote = c;
                        charIndex++; // Move past opening quote
                        while (charIndex < line.Length)
                        {
                            if (line[charIndex] == quote && (charIndex == 0 || line[charIndex - 1] != '\\'))
                            {
                                break; // Found closing quote
                            }
                            charIndex++;
                        }
                        continue;
                    }

                    // Skip single-line comments
                    if (c == '/' && charIndex + 1 < line.Length && line[charIndex + 1] == '/')
                    {
                        break; // Skip rest of line
                    }

                    // Skip multi-line comments
                    if (c == '/' && charIndex + 1 < line.Length && line[charIndex + 1] == '*')
                    {
                        charIndex += 2;
                        bool foundEndComment = false;
                        while (lineIndex < lines.Length && !foundEndComment)
                        {
                            while (charIndex < lines[lineIndex].Length - 1)
                            {
                                if (lines[lineIndex][charIndex] == '*' && lines[lineIndex][charIndex + 1] == '/')
                                {
                                    charIndex += 2;
                                    foundEndComment = true;
                                    break;
                                }
                                charIndex++;
                            }
                            if (!foundEndComment)
                            {
                                lineIndex++;
                                charIndex = 0;
                                if (lineIndex >= lines.Length)
                                {
                                    break;
                                }
                            }
                        }
                        if (foundEndComment)
                        {
                            charIndex--; // Adjust for loop increment
                        }
                        continue;
                    }

                    if (c == '{')
                    {
                        openBraces++;
                    }
                    else if (c == '}')
                    {
                        openBraces--;
                        if (openBraces == 0)
                        {
                            closeBraceLine = lineIndex;
                            closeBraceColumn = charIndex;
                            foundClosing = true;
                            break; // break inner loop
                        }
                    }
                }

                if (foundClosing)
                {
                    break; // break outer loop
                }
            }

            // If no matching closing brace found, return empty
            if (closeBraceLine == -1)
            {
                return string.Empty;
            }

            // Extract the method body from opening brace to closing brace (inclusive)
            StringBuilder methodBody = new StringBuilder();

            for (int lineIndex = braceStartLine; lineIndex <= closeBraceLine; lineIndex++)
            {
                string line = lines[lineIndex];

                if (lineIndex == braceStartLine && lineIndex == closeBraceLine)
                {
                    // Opening and closing braces are on the same line
                    methodBody.AppendLine(line.Substring(braceStartColumn, closeBraceColumn - braceStartColumn + 1));
                }
                else if (lineIndex == braceStartLine)
                {
                    // First line (contains opening brace)
                    methodBody.AppendLine(line.Substring(braceStartColumn));
                }
                else if (lineIndex == closeBraceLine)
                {
                    // Last line (contains closing brace)
                    methodBody.AppendLine(line.Substring(0, closeBraceColumn + 1));
                }
                else
                {
                    // Middle lines
                    methodBody.AppendLine(line);
                }
            }

            return methodBody.ToString().Trim();
        }

        /// <summary>
        /// Counts the balance of opening and closing delimiters in a line.
        /// </summary>
        private int CountDelimiters(string line)
        {
            int balance = 0;

            foreach (char c in line)
            {
                if (OpeningDelimiters.Contains(c))
                {
                    balance++;
                }
                else if (ClosingDelimiters.Contains(c))
                {
                    balance--;
                }
            }

            return balance;
        }
    }
}
