using CodeReadabilityLib.Extractors;
using CodeReadabilityLib.Extractors.Values;
using CodeReadabilityLib.Judger;
using CodeReadabilityLib.Metrics;
using CodeReadabilityLib.Savers;
using CodeReadabilityLib.Scrapers;
using CodeReadabilityLib.Scrapers.Values;

namespace CodeReadabilityLib
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

            // Determine language and applicable metrics
            SupportedLanguage language = DetermineLanguage(fileExtension);
            MetricDefinition[] llmMetrics = MetricRegistry.GetLlmBasedForLanguage(language).ToArray();
            MetricDefinition[] algorithmicMetrics = MetricRegistry.GetAlgorithmicForLanguage(language).ToArray();

            _logger?.WriteLine($"Using {llmMetrics.Length} LLM-based metrics and {algorithmicMetrics.Length} algorithmic metrics.");

            // Evaluate each extracted element
            Dictionary<IExtractedData, List<ReadabilityScore>> evaluatedResults = new();

            int processedCount = 0;
            foreach (IExtractedData data in allExtractedData)
            {
                if (ctoken.IsCancellationRequested) break;

                try
                {
                    string codeToEvaluate = ExtractCodeContent(data);

                    // Step 1: LLM-based metrics
                    ReadabilityEvaluationRequest request = new ReadabilityEvaluationRequest(
                        language.ToString(),
                        null,
                        codeToEvaluate,
                        llmMetrics);

                    IReadOnlyList<ReadabilityScore> llmScores = await _readabilityJudge.EvaluateAsync(request, ctoken);

                    if (llmScores.Count > 0)
                    {
                        evaluatedResults.Add(data, llmScores.ToList());
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

            _logger?.WriteLine($"Evaluation complete. Processed {evaluatedResults.Count}/{allExtractedData.Count} elements.");

            // Save results to JSON
            _logger?.WriteLine($"Saving results to: {outputPath}");
            await _dataSaver.SaveAllResultsAsync(evaluatedResults, outputPath);

            _logger?.WriteLine("Done!");
        }

        private string ExtractCodeContent(IExtractedData data)
        {
            return data switch
            {
                MethodData method => method.MethodBody,
                _ => data.ToString() ?? string.Empty
            };
        }

        private SupportedLanguage DetermineLanguage(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".py" => SupportedLanguage.Python,
                ".cs" => SupportedLanguage.CSharp,
                _ => throw new ArgumentException($"Unsupported language: {fileExtension}")
            };
        }
    }
}
