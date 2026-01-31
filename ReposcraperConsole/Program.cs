using Reposcraper.Extractors;
using Reposcraper.Extractors.PatternMatcher;
using Reposcraper.Judger;
using Reposcraper.Savers;
using Reposcraper.Scrapers;
using static ReposcraperConsole.Arguments;

namespace ReposcraperConsole
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                Dictionary<string, string> arguments = ParseArguments(args);

                // If asking for help
                if (arguments.ContainsKey(ARG_HELP) || arguments.ContainsKey(ARG_HELP_SHORT))
                {
                    ShowHelp();
                    return 0;
                }

                // Validate required arguments
                if (!ValidateRequiredArguments(arguments, out string? error))
                {
                    Console.WriteLine($"Error: {error}");
                    Console.WriteLine();
                    ShowHelp();
                    return 1;
                }

                // Extract values
                string ggufPath = arguments[ARG_GGUF_PATH];
                string repoUrl = arguments[ARG_REPO_URL];
                string language = arguments[ARG_LANGUAGE];
                string extractionType = arguments[ARG_EXTRACTION_TYPE];
                string outputPath = arguments[ARG_OUTPUT_PATH];

                // Get argument values or defaults
                int port = arguments.TryGetValue(ARG_PORT, out string? portStr)
                   ? int.Parse(portStr)
                   : DEFAULT_PORT;

                int gpuLayers = arguments.TryGetValue(ARG_GPU_LAYERS, out string? gpuStr)
                    ? int.Parse(gpuStr)
                    : DEFAULT_GPU_LAYERS;

                // TODO: implement other tokens aswell
                string? githubToken = arguments.TryGetValue(ARG_GITHUB_TOKEN, out string? token)
                    ? token
                    : null; 

                await ExecuteAsync(ggufPath, repoUrl, language, extractionType, outputPath, port, gpuLayers, githubToken, CancellationToken.None);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
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

        private static bool ValidateRequiredArguments(Dictionary<string, string> arguments, out string? error)
        {
            foreach (string arg in RequiredArguments)
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

        private static void ShowHelp()
        {
            Console.WriteLine("Reposcraper - Extract and evaluate code readability");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  ReposcraperConsole [options]");
            Console.WriteLine();
            Console.WriteLine("Required Options:");
            Console.WriteLine($"  --{ARG_GGUF_PATH} <path>          Path to the GGUF model file");
            Console.WriteLine($"  --{ARG_REPO_URL} <url>            URL of the repository to analyze");
            Console.WriteLine($"  --{ARG_LANGUAGE} <language>       Programming language: Python, CSharp, C++, C");
            Console.WriteLine($"  --{ARG_EXTRACTION_TYPE} <type>    Type of extraction: methods, classes, or files");
            Console.WriteLine($"  --{ARG_OUTPUT_PATH} <path>        Path where the JSON results should be saved");
            Console.WriteLine();
            Console.WriteLine("Optional Options:");
            Console.WriteLine($"  --{ARG_PORT} <number>             Port for the llama-server (default: {DEFAULT_PORT})");
            Console.WriteLine($"  --{ARG_GPU_LAYERS} <number>       Number of layers to offload to GPU (default: {DEFAULT_GPU_LAYERS})");
            Console.WriteLine($"  --{ARG_GITHUB_TOKEN} <token>      GitHub Personal Access Token");
            Console.WriteLine();
            Console.WriteLine("Other:");
            Console.WriteLine($"  --{ARG_HELP}, -{ARG_HELP_SHORT}                  Show this help message");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine($"  ReposcraperConsole --{ARG_GGUF_PATH} model.gguf --{ARG_REPO_URL} https://github.com/user/repo");
            Console.WriteLine($"                     --{ARG_LANGUAGE} Python --{ARG_EXTRACTION_TYPE} methods --{ARG_OUTPUT_PATH} results.json");
        }

        private static async Task ExecuteAsync(string ggufPath, string repoUrl, string language, string extractionType, string outputPath,
            int port, int gpuLayers, string? githubToken, CancellationToken ctoken)
        {
            Console.WriteLine($"Reposcraper v{Reposcraper.Reposcraper.Version}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"GGUF Model:      {ggufPath}");
            Console.WriteLine($"Repository:      {repoUrl}");
            Console.WriteLine($"Language:        {language}");
            Console.WriteLine($"Extraction Type: {extractionType}");
            Console.WriteLine($"Output Path:     {outputPath}");
            Console.WriteLine($"Port:            {port}");
            Console.WriteLine($"GPU Layers:      {gpuLayers}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            GgufJudge? judge = null;

            try
            {
                // Validate inputs
                if (!File.Exists(ggufPath))
                {
                    throw new FileNotFoundException($"GGUF file not found: {ggufPath}");
                }

                string fileExtension = GetFileExtension(language);

                // Start GGUF LLM
                Console.WriteLine("Starting GGUF LLM server...");
                judge = new GgufJudge(ggufPath, port, gpuLayers);
                await judge.StartLlamaAsync();
                Console.WriteLine("✓ LLM server started successfully.");
                Console.WriteLine();

                // Create components
                GitHubScraper scraper = new GitHubScraper(githubToken);
                IPatternMatcher patternMatcher = GetPatternMatcher(language);
                IDataExtractor extractor = GetExtractor(extractionType, patternMatcher);

                // Create and execute Reposcraper
                Reposcraper.Reposcraper reposcraper = new Reposcraper.Reposcraper(scraper, extractor, judge, new JsonDataSaver());
                await reposcraper.Execute(repoUrl, fileExtension, outputPath, ctoken);

                Console.WriteLine();
                Console.WriteLine("✓ Execution completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"✗ Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
                }
#if DEBUG
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
#endif
                throw;
            }
            finally
            {
                // Stop the LLM server
                if (judge != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Stopping GGUF LLM server...");
                    judge.StopLlama();
                    judge.Dispose();
                    Console.WriteLine("✓ LLM server stopped.");
                }
            }
        }

        private static string GetFileExtension(string language)
        {
            return language.ToLowerInvariant() switch
            {
                "python" => ".py",
                "csharp" or "c#" or "cs" => ".cs",
                "cpp" or "c++" => ".cpp",
                "c" => ".c",
                _ => throw new ArgumentException($"Unsupported language: {language}. Supported: Python, CSharp, Java, JavaScript, TypeScript, C++, C, Go, Rust, Ruby, PHP, Swift, Kotlin")
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