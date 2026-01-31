namespace ReposcraperConsole
{
    public static class Arguments
    {
        // Required arguments
        public const string ARG_GGUF_PATH = "gguf-path";
        public const string ARG_REPO_URL = "repo-url";
        public const string ARG_LANGUAGE = "language";
        public const string ARG_EXTRACTION_TYPE = "extraction-type";
        public const string ARG_OUTPUT_PATH = "output-path";

        // Optional arguments
        public const string ARG_PORT = "port";
        public const string ARG_GPU_LAYERS = "gpu-layers";
        public const string ARG_GITHUB_TOKEN = "github-token";

        // Help arguments
        public const string ARG_HELP = "help";
        public const string ARG_HELP_SHORT = "h";


        // Argument collections
        public static readonly IReadOnlyList<string> RequiredArguments =
        [
            ARG_GGUF_PATH,
            ARG_REPO_URL,
            ARG_LANGUAGE,
            ARG_EXTRACTION_TYPE,
            ARG_OUTPUT_PATH
        ];

        public static readonly IReadOnlyList<string> OptionalArguments =
        [
            ARG_PORT,
            ARG_GPU_LAYERS,
            ARG_GITHUB_TOKEN
        ];

        // Default values
        public const int DEFAULT_PORT = 8080;
        public const int DEFAULT_GPU_LAYERS = 99;
    }
}
