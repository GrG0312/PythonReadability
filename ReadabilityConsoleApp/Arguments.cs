namespace ReadabilityConsoleApp
{
    public static class Arguments
    {
        public const string ARG_HELP = "help";

        // Mode arguments
        public const string ARG_MODE = "mode";

        // Shared / Output
        public const string ARG_OUTPUT_PATH = "output-path";

        // Metric Evaluation Required arguments
        public const string ARG_GGUF_PATH = "gguf-path";
        public const string ARG_REPO_URL = "repo-url";
        public const string ARG_LANGUAGE = "language";
        public const string ARG_EXTRACTION_TYPE = "extraction-type";

        // Model Evaluation Required arguments
        public const string ARG_DATASET_PATH = "dataset-path";
        public const string ARG_GGUF_DIR = "gguf-dir";

        // Optional arguments
        public const string ARG_PORT = "port";
        public const string ARG_GPU_LAYERS = "gpu-layers";
        public const string ARG_GITHUB_TOKEN = "github-token";

        public const string ARG_MIN_SCORE = "min-score";
        public const string ARG_MAX_SCORE = "max-score";
        public const string ARG_LLAMA_SERVER_PATH = "llama-server-path";

        // Argument collections by mode
        public static readonly IReadOnlyList<string> MetricRequiredArguments =
        [
            ARG_GGUF_PATH,
            ARG_REPO_URL,
            ARG_LANGUAGE,
            ARG_EXTRACTION_TYPE,
            ARG_OUTPUT_PATH,
        ];

        public static readonly IReadOnlyList<string> ModelRequiredArguments =
        [
            ARG_DATASET_PATH,
            ARG_GGUF_DIR,
            ARG_OUTPUT_PATH,
        ];

        public static readonly IReadOnlyList<string> OptionalArguments =
        [
            ARG_PORT,
            ARG_GPU_LAYERS,
            ARG_GITHUB_TOKEN,
            ARG_MIN_SCORE,
            ARG_MAX_SCORE,
            ARG_LLAMA_SERVER_PATH
        ];

        // Default values
        public const int DEFAULT_PORT = 8080;
        public const int DEFAULT_GPU_LAYERS = 99;
        public const int DEFAULT_MIN_SCORE = 1;
        public const int DEFAULT_MAX_SCORE = 5;
        public const string MODE_METRIC = "metric"; // evaluates metrics
        public const string MODE_MODEL = "model"; // evaluates models
    }
}
