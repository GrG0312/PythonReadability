using Reposcraper.Extractors;
using Reposcraper.Savers;

namespace Reposcraper
{
    public readonly struct ExtractionRequest
    {
        public readonly string RepositoryUrl;
        //public readonly IDataExtractor Extractor;
        public readonly IDataSaver Saver;
        public readonly string OutputPath;
        public readonly string FileExtension;

        public ExtractionRequest(
            string repositoryUrl,
            //IDataExtractor extractor,
            IDataSaver saver,
            string outputPath,
            string fileExtension)
        {
            RepositoryUrl = repositoryUrl;
            //Extractor = extractor;
            Saver = saver;
            OutputPath = outputPath;
            FileExtension = fileExtension;
        }
    }
}
