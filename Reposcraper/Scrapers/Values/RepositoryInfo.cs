namespace Reposcraper.Scrapers.Values
{
    public record RepositoryInfo
    {
        public readonly string RepoName;
        public readonly string Owner;
        public readonly string ApiUrl;
        public readonly string WebUrl;

        public RepositoryInfo(string repoName, string owner, string apiUrl, string webUrl)
        {
            RepoName = repoName;
            Owner = owner;
            ApiUrl = apiUrl;
            WebUrl = webUrl;
        }
    }
}
