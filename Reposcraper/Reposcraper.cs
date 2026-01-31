using Reposcraper.Extractors;
using Reposcraper.Extractors.Values;
using Reposcraper.Judger;
using Reposcraper.Savers;
using Reposcraper.Scrapers;
using Reposcraper.Scrapers.Values;

namespace Reposcraper
{
    /// <summary>
    /// Main execution class for the Reposcraper application.
    /// </summary>
    public class Reposcraper
    {
        public const string Version = "1.0.0";

        private readonly IRepositoryScraper _scraper;
        private readonly IDataExtractor _dataExtractor;
        private readonly IReadabilityJudge _readabilityJudge;
        private readonly IDataSaver _dataSaver;
        private readonly ILogger? _logger;

        public Reposcraper(
            IRepositoryScraper scraper,
            IDataExtractor dataExtractor,
            IReadabilityJudge readabilityJudge,
            IDataSaver dataSaver,
            ILogger? logger = null)
        {
            _scraper = scraper;
            _dataExtractor = dataExtractor;
            _readabilityJudge = readabilityJudge;
            _dataSaver = dataSaver;
            _logger = logger;
        }

        public async Task Execute(string repositoryUrl, string fileExtension, string outputPath, CancellationToken ctoken = default)
        {
            if (!_scraper.CanScrape(repositoryUrl))
            {
                throw new ArgumentException("Invalid repository URL.");
            }

            _logger?.WriteLine($"Fetching repository information from: {repositoryUrl}");
            RepositoryInfo info = await _scraper.GetRepositoryInfoAsync(repositoryUrl);

            _logger?.WriteLine($"Scanning for files with extension: {fileExtension}");
            RepositoryFile[] files = await _scraper.GetFilesAsync(info, fileExtension);
            _logger?.WriteLine($"Found {files.Length} files to process.");


            // Extract data from all files
            _logger?.WriteLine("Extracting code elements...");
            IReadOnlyCollection<ExtractionResult> extractionResults = await _dataExtractor.ExtractAsync(files);


            // Collect all extracted data items
            List<IExtractedData> allExtractedData = extractionResults
                .Where(result => result.IsSuccessful)
                .SelectMany(result => result.ExtractedData)
                .ToList();

            _logger?.WriteLine($"Extracted {allExtractedData.Count} code elements.");


            // Evaluate each extracted element
            _logger?.WriteLine("Evaluating readability with LLM...");
            Dictionary<IExtractedData, List<ReadabilityScore>> evaluatedResults = new Dictionary<IExtractedData, List<ReadabilityScore>>();

            int processedCount = 0;
            foreach (IExtractedData data in allExtractedData)
            {
                if (ctoken.IsCancellationRequested) break;

                try
                {
                    string codeToEvaluate = ExtractCodeContent(data);

                    // TODO: add context and more metrics
                    ReadabilityEvaluationRequest request = new ReadabilityEvaluationRequest(
                        DetermineLanguage(fileExtension), 
                        null, 
                        codeToEvaluate, 
                        new [] { "LLM_readability" });

                    IReadOnlyList<ReadabilityScore> scores = await _readabilityJudge.EvaluateAsync(request, ctoken);

                    if (scores.Count > 0)
                    {
                        evaluatedResults.Add(data, scores.ToList());
                    }

                    processedCount++;
                    if (processedCount % 10 == 0)
                    {
                        _logger?.WriteLine($"Processed {processedCount}/{allExtractedData.Count} elements...");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.WriteLine($"Error evaluating element: {ex.Message}");
                }
            }

            _logger?.WriteLine($"Evaluation complete. Processed {evaluatedResults.Count} elements.");

            // Save results to JSON
            Console.WriteLine($"Saving results to: {outputPath}");
            await AggregatedJsonSaver.SaveAllResultsAsync(evaluatedResults, outputPath);

            Console.WriteLine("Done!");
        }

        private string ExtractCodeContent(IExtractedData data)
        {
            return data switch
            {
                MethodData method => method.MethodBody,
                _ => data.ToString() ?? string.Empty
            };
        }

        private string DetermineLanguage(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".py" => "Python",
                ".cs" => "C#",
                ".java" => "Java",
                ".cpp" or ".cc" or ".cxx" => "C++",
                ".c" => "C",
                _ => "Unknown"
            };
        }
    }
}
