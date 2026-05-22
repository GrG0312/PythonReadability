namespace CodeReadabilityLib.Datasets
{
    public record CodeSnippetSample
    {
        public readonly string Id;
        public readonly string Code;
        public readonly double Score;

        public CodeSnippetSample(string id, string code, double score)
        {
            Id = id;
            Code = code;
            Score = score;
        }
    }
}
