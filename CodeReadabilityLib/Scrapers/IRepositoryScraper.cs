using CodeReadabilityLib.Scrapers.Values;

namespace CodeReadabilityLib.Scrapers
{
    public interface IRepositoryScraper
    {
        public bool CanScrape(string repositoryUrl);
        public Task<RepositoryInfo> GetRepositoryInfoAsync(string repositoryUrl);
        public Task<RepositoryFile[]> GetFilesAsync(RepositoryInfo info, string fileExtension);
    }
}