namespace Reposcraper.Scrapers
{
    public static class ScraperFactory
    {
        private readonly static IRepositoryScraper[] scrapers = new IRepositoryScraper[]
        {
            new GitHubScraper(),
            //new GitLabScraper(),
            //new HuggingFaceScraper()
        };

        public static IRepositoryScraper GetScraper(string repositoryUrl)
        {
            return scrapers.FirstOrDefault(scraper => scraper.CanScrape(repositoryUrl))
                ?? throw new NotSupportedException($"No scraper available for the repository URL: {repositoryUrl}");
        }
    }
}
