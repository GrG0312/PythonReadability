using Reposcraper.Extractors.PatternMatcher;
using Reposcraper.Extractors.Values;
using Reposcraper.Scrapers.Values;
using System.Text.RegularExpressions;

namespace Reposcraper.Extractors
{
    public class MethodExtractor : ExtractorBase
    {
        public MethodExtractor(IPatternMatcher patternMatcher) : base(patternMatcher) { }

        /// <summary>
        /// Determines if the extractor can handle the given file extension based on the contained pattern matcher object.
        /// </summary>
        /// <param name="fileExtension">The file extension to check.</param>
        /// <returns>True if the extractor can handle the file extension; otherwise, false.</returns>
        public bool CanHandle(string fileExtension)
        {
            return _patternMatcher.CanHandle(fileExtension);
        }

        /// <summary>
        /// Asynchronously extracts method definitions and their associated documentation comments from the specified
        /// repository file by downloading its content.
        /// </summary>
        /// <remarks>
        /// The extraction process identifies methods using pattern matching and includes any documentation comments that
        /// immediately precede each method. The operation is performed asynchronously and is safe to call concurrently.
        /// If the file content is empty or whitespace, the result will be successful but contain no extracted methods.
        /// </remarks>
        /// <param name="file">The repository file from which methods are to be extracted.</param>
        /// <returns>A task that represents the asynchronous extraction operation. The result contains an ExtractionResult
        /// with a collection of MethodData objects for each extracted method. If no methods are found or the content is empty,
        /// the collection will be empty. If an error occurs, the result will indicate failure and include an error message.</returns>
        public override async Task<ExtractionResult> ExtractAsync(RepositoryFile file)
        {
            if (!CanHandle(file.GetExtension()))
            {
                return new ExtractionResult(
                    file,
                    isSuccessful: false,
                    extractedData: [],
                    errorMessage: $"Cannot handle files with extension: {file.GetExtension()}");
            }

            try
            {
                string fileContent = await DownloadFileContentAsync(file);

                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return new ExtractionResult(
                        file,
                        isSuccessful: true,
                        extractedData: []);
                }

                List<MethodData> extractedMethods = new List<MethodData>();
                string[] lines = fileContent.Split('\n');

                // Find all method matches using the pattern matcher
                MatchCollection methodMatches = _patternMatcher.MethodRegex.Matches(fileContent);

                foreach (Match methodMatch in methodMatches)
                {
                    // Get the line number where the method starts
                    int methodStartIndex = methodMatch.Index;
                    int lineNumber = GetLineNumber(fileContent, methodStartIndex);

                    // Extract comments/documentation that appear directly before the method
                    string precedingComments = ExtractPrecedingComments(lines, lineNumber);

                    // Extract the complete method (signature + body)
                    string methodBody = ExtractCompleteMethod(fileContent, methodMatch, lines, lineNumber);

                    // Clean up the method signature
                    string signature = methodMatch.Value.Trim().TrimEnd(Environment.NewLine.ToCharArray());

                    // Create an extracted method data object
                    MethodData extractedMethod = new MethodData(
                        methodSignature: signature,
                        methodBody: methodBody,
                        precedingComments: precedingComments,
                        sourceFile: file);

                    extractedMethods.Add(extractedMethod);
                }

                return new ExtractionResult(
                    file,
                    isSuccessful: true,
                    extractedData: extractedMethods);
            }
            catch (Exception ex)
            {
                return new ExtractionResult(
                    file,
                    isSuccessful: false,
                    extractedData: [],
                    errorMessage: $"Error extracting methods: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads the full text content of the repository file using its DownloadURL.
        /// </summary>
        /// <param name="file">The repository file to download.</param>
        /// <returns>The downloaded file content as a string.</returns>
        private async Task<string> DownloadFileContentAsync(RepositoryFile file)
        {
            if (string.IsNullOrWhiteSpace(file.DownloadURL))
            {
                return string.Empty;
            }

            using HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            return await httpClient.GetStringAsync(file.DownloadURL);
        }

        /// <summary>
        /// Gets the line number in the content corresponding to the given character index.
        /// </summary>
        /// <param name="content">The content to search within.</param>
        /// <param name="characterIndex">The character index to find the line number for.</param>
        /// <returns>The line number (0-based) corresponding to the character index.</returns>
        private int GetLineNumber(string content, int characterIndex)
        {
            return content.Take(characterIndex).Count(c => c == '\n');
        }

        /// <summary>
        /// Extracts comments that directly precede a method definition.
        /// </summary>
        /// <param name="lines">Each line of the file content.</param>
        /// <param name="methodLineIndex">The line index of the method definition.</param>
        /// <returns>The extracted comments as a single string.</returns>
        private string ExtractPrecedingComments(string[] lines, int methodLineIndex)
        {
            List<string> comments = new List<string>();

            // Look backwards from the method line to find consecutive comments
            for (int i = methodLineIndex - 1; i >= 0; i--)
            {
                string line = lines[i].Trim();

                // Check if this line is a comment using the comment regex
                if (_patternMatcher.CommentRegex.IsMatch(line))
                {
                    comments.Insert(0, lines[i]); // Insert at beginning to maintain order
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // Allow empty lines between comments and method
                    continue;
                }
                else
                {
                    // Stop when we hit non-comment, non-empty line
                    break;
                }
            }

            return string.Join('\n', comments);
        }

        /// <summary>
        /// Extracts the complete source code of a method from the specified file content using the provided match and
        /// line information.
        /// </summary>
        /// <param name="fileContent">The full text content of the source file from which the method is to be extracted.</param>
        /// <param name="methodMatch">A regular expression match that identifies the starting point of the method within the file content.</param>
        /// <param name="lines">An array of strings representing the individual lines of the source file.</param>
        /// <param name="startLineNumber">The zero-based line number indicating where the method begins in the file.</param>
        /// <returns>A string containing the complete source code of the extracted method, including its signature and body.</returns>
        private string ExtractCompleteMethod(string fileContent, Match methodMatch, string[] lines, int startLineNumber)
        {
            return _patternMatcher.MethodDelimitationStrategy.ExtractMethod(methodMatch, lines, startLineNumber, _patternMatcher);
        }
    }
}
