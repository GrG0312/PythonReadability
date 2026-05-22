using CodeReadabilityLib;
using CodeReadabilityLib.Evaluation;
using CodeReadabilityLib.Evaluators;
using CodeReadabilityLib.Extractors;
using CodeReadabilityLib.Extractors.PatternMatcher;
using CodeReadabilityLib.Judger;
using CodeReadabilityLib.Savers;
using CodeReadabilityLib.Scrapers;
using System.Text.Json;
using static ReadabilityConsoleApp.Arguments;

namespace ReadabilityConsoleApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                Dictionary<string, string> arguments = ParseArguments(args);

                // If asking for help
                if (arguments.ContainsKey(ARG_HELP))
                {
                    ShowHelp();
                    return 0;
                }

                // Determine the mode (default to metric evaluation)
                string mode = arguments.TryGetValue(ARG_MODE, out string? parsedMode)
                    ? parsedMode.ToLowerInvariant()
                    : MODE_METRIC;

                if (mode == MODE_METRIC)
                {
                    if (!ValidateRequiredArguments(arguments, MetricRequiredArguments, out string? error))
                    {
                        Console.WriteLine($"Error: {error}");
                        Console.WriteLine();
                        ShowHelp();
                        return 1;
                    }

                    string ggufPath = arguments[ARG_GGUF_PATH];
                    string repoUrl = arguments[ARG_REPO_URL];
                    string language = arguments[ARG_LANGUAGE];
                    string extractionType = arguments[ARG_EXTRACTION_TYPE];
                    string outputPath = arguments[ARG_OUTPUT_PATH];

                    int port = GetIntOrDefault(arguments, ARG_PORT, DEFAULT_PORT);
                    int gpuLayers = GetIntOrDefault(arguments, ARG_GPU_LAYERS, DEFAULT_GPU_LAYERS);
                    string? githubToken = arguments.GetValueOrDefault(ARG_GITHUB_TOKEN);

                    await ExecuteMetricAsync(ggufPath, repoUrl, language, extractionType, outputPath, port, gpuLayers, githubToken, CancellationToken.None);
                }
                else if (mode == MODE_MODEL)
                {
                    if (!ValidateRequiredArguments(arguments, ModelRequiredArguments, out string? error))
                    {
                        Console.WriteLine($"Error: {error}");
                        Console.WriteLine();
                        ShowHelp();
                        return 1;
                    }

                    string datasetPath = arguments[ARG_DATASET_PATH];
                    string ggufDir = arguments[ARG_GGUF_DIR]; // Fixed: using ARG_GGUF_DIR
                    string outputPath = arguments[ARG_OUTPUT_PATH];

                    int port = GetIntOrDefault(arguments, ARG_PORT, DEFAULT_PORT);
                    int gpuLayers = GetIntOrDefault(arguments, ARG_GPU_LAYERS, DEFAULT_GPU_LAYERS);

                    // Fixed: Parse min/max scores with defaults fallback
                    int minScore = GetIntOrDefault(arguments, ARG_MIN_SCORE, DEFAULT_MIN_SCORE);
                    int maxScore = GetIntOrDefault(arguments, ARG_MAX_SCORE, DEFAULT_MAX_SCORE);

                    await ExecuteModelEvaluationAsync(
                        datasetPath: datasetPath,
                        ggufDir: ggufDir,
                        outputPath: outputPath,
                        minScore: minScore,
                        maxScore: maxScore,
                        port: port,
                        gpuLayers: gpuLayers,
                        ctoken: CancellationToken.None);
                }
                else
                {
                    Console.WriteLine($"Error: Unknown mode '{mode}'. Use 'metric' or 'model'.");
                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
#if DEBUG
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
#endif
                return 1;
            }
        }

        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.StartsWith("--"))
                {
                    string key = arg[2..];

                    // Check if next argument is the value
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        arguments[key] = args[i + 1];
                        i++; // Skip the value in next iteration
                    }
                    else
                    {
                        // Flag without value
                        arguments[key] = "true";
                    }
                }
                else if (arg.StartsWith('-'))
                {
                    string key = arg[1..];

                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                    {
                        arguments[key] = args[i + 1];
                        i++;
                    }
                    else
                    {
                        arguments[key] = "true";
                    }
                }
            }

            return arguments;
        }

        private static bool ValidateRequiredArguments(Dictionary<string, string> arguments, IReadOnlyList<string> requiredList, out string? error)
        {
            foreach (string arg in requiredList)
            {
                if (!arguments.TryGetValue(arg, out string? value) || string.IsNullOrWhiteSpace(value))
                {
                    error = $"Missing required argument: --{arg}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static int GetIntOrDefault(Dictionary<string, string> arguments, string key, int defaultValue)
        {
            return arguments.TryGetValue(key, out string? strVal) && int.TryParse(strVal, out int val)
                ? val
                : defaultValue;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Reposcraper - Extract and evaluate code readability");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  ReposcraperConsole --mode <metric|model> [options]");
            Console.WriteLine();
            Console.WriteLine("====== MODE: METRIC (Default) ======");
            Console.WriteLine($"  Evaluate readability metrics on a GitHub repository.");
            Console.WriteLine($"  --{ARG_GGUF_PATH} <path>          Path to the GGUF model file");
            Console.WriteLine($"  --{ARG_REPO_URL} <url>            URL of the repository to analyze");
            Console.WriteLine($"  --{ARG_LANGUAGE} <language>       Programming language: Python, CSharp, C++, C");
            Console.WriteLine($"  --{ARG_EXTRACTION_TYPE} <type>    Type of extraction: methods, classes, or files");
            Console.WriteLine($"  --{ARG_OUTPUT_PATH} <path>        Path where the JSON results should be saved");
            Console.WriteLine();
            Console.WriteLine("====== MODE: MODEL ======");
            Console.WriteLine($"  Rank models against a dataset of human readability scores (1-5).");
            Console.WriteLine($"  --{ARG_DATASET_PATH} <path>       Path to the CSV/JSON dataset");
            Console.WriteLine($"  --{ARG_GGUF_PATH} <paths>         A path to the directory containing the GGUF files");
            Console.WriteLine($"  --{ARG_OUTPUT_PATH} <path>        Path where the evaluation report should be saved");
            Console.WriteLine();
            Console.WriteLine("Optional Options:");
            Console.WriteLine($"  --{ARG_PORT} <number>             Port for the llama-server (default: {DEFAULT_PORT})");
            Console.WriteLine($"  --{ARG_GPU_LAYERS} <number>       Number of layers to offload to GPU (default: {DEFAULT_GPU_LAYERS})");
            Console.WriteLine($"  --{ARG_GITHUB_TOKEN} <token>      GitHub Personal Access Token (for Repo Scraping)");
            Console.WriteLine();
            Console.WriteLine("Example Metric:");
            Console.WriteLine($"  ReposcraperConsole --mode metric --{ARG_GGUF_PATH} model.gguf --{ARG_REPO_URL} https://github.com/user/repo --{ARG_LANGUAGE} Python --{ARG_EXTRACTION_TYPE} methods --{ARG_OUTPUT_PATH} results.json");
            Console.WriteLine("Example Model:");
            Console.WriteLine($"  ReposcraperConsole --mode model --{ARG_DATASET_PATH} ds.csv --{ARG_GGUF_PATH} \"m1.gguf;m2.gguf\" --{ARG_OUTPUT_PATH} report.json");
        }

        private static async Task ExecuteMetricAsync(string ggufPath, string repoUrl, string language, string extractionType, string outputPath,
            int port, int gpuLayers, string? githubToken, CancellationToken ctoken)
        {
            Console.WriteLine($"Reposcraper v{Reposcraper.Version} (Metric Mode)");
            Console.WriteLine(new string('=', 60));
            // Logging ...

            GgufJudge? judge = null;
            try
            {
                if (!File.Exists(ggufPath))
                    throw new FileNotFoundException($"GGUF file not found: {ggufPath}");

                string fileExtension = GetFileExtension(language);

                Console.WriteLine("Starting GGUF LLM server...");
                judge = new GgufJudge(ggufPath, port, gpuLayers);
                await judge.StartLlamaAsync();
                Console.WriteLine("LLM server started successfully.\n");

                GitHubScraper scraper = new GitHubScraper(githubToken);
                IPatternMatcher patternMatcher = GetPatternMatcher(language);
                IDataExtractor extractor = GetExtractor(extractionType, patternMatcher);
                IDataSaver saver = new JsonDataSaver();

                Reposcraper reposcraper = new Reposcraper(scraper, extractor, judge, saver);
                await reposcraper.Execute(repoUrl, fileExtension, outputPath, ctoken);

                Console.WriteLine("\nExecution completed successfully!");
            }
            finally
            {
                if (judge != null)
                {
                    Console.WriteLine("\nStopping GGUF LLM server...");
                    judge.StopLlama();
                    judge.Dispose();
                    Console.WriteLine("LLM server stopped.");
                }
            }
        }

        private static async Task ExecuteModelEvaluationAsync(string datasetPath, string ggufDir, string outputPath,
    int minScore, int maxScore, int port, int gpuLayers, CancellationToken ctoken)
        {
            Console.WriteLine($"Reposcraper v{Reposcraper.Version} (Model Evaluation Mode)");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Dataset:     {datasetPath}");
            Console.WriteLine($"Model Dir:   {ggufDir}");
            Console.WriteLine($"Score Range: {minScore} to {maxScore}");
            Console.WriteLine($"Output Path: {outputPath}");
            Console.WriteLine(new string('=', 60));

            // Call the newly created service
            ModelRankingService service = new ModelRankingService(port, gpuLayers);

            EvaluationReport rankedReport =
                await service.EvaluateAsync(datasetPath, ggufDir, minScore, maxScore, ctoken);

            // Save strictly matching the example format
            string reportJson = JsonSerializer.Serialize(rankedReport, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            string? dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(outputPath, reportJson, ctoken);

            Console.WriteLine("\nModel evaluation and comparison completed successfully!");
        }

        private static string GetFileExtension(string language)
        {
            return language.ToLowerInvariant() switch
            {
                "python" => ".py",
                "csharp" or "c#" or "cs" => ".cs",
                _ => throw new ArgumentException($"Unsupported language: {language}. Supported: Python, CSharp")
            };
        }

        #region Factory methods
        private static IPatternMatcher GetPatternMatcher(string language)
        {
            return language.ToLowerInvariant() switch
            {
                "python" => new PythonPatternMatcher(),
                "csharp" or "c#" or "cs" => new CSharpPatternMatcher(),
                _ => throw new ArgumentException($"No pattern matcher available for language: {language}")
            };
        }

        private static IDataExtractor GetExtractor(string extractionType, IPatternMatcher patternMatcher)
        {
            return extractionType.ToLowerInvariant() switch
            {
                "methods" or "method" => new MethodExtractor(patternMatcher),
                "classes" or "class" => throw new NotImplementedException("Class extraction not yet implemented. Please implement ClassExtractor."),
                "files" or "file" => throw new NotImplementedException("File extraction not yet implemented. Please implement FileExtractor."),
                _ => throw new ArgumentException($"Invalid extraction type: {extractionType}. Valid options: methods, classes, files")
            };
        }
        #endregion
    }
}