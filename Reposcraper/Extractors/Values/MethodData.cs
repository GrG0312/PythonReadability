using Reposcraper.Scrapers.Values;

namespace Reposcraper.Extractors.Values
{
    public record MethodData : IExtractedData
    {
        public readonly string MethodSignature;
        public readonly string MethodBody;
        public readonly string PrecedingComments;
        public readonly RepositoryFile SourceFile;

        public MethodData(
            RepositoryFile sourceFile,
            string methodSignature,
            string methodBody,
            string precedingComments)
        {
            SourceFile = sourceFile;
            MethodSignature = methodSignature;
            MethodBody = methodBody;
            PrecedingComments = precedingComments;
        }
    }
}
