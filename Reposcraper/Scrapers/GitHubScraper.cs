using Reposcraper.Scrapers.Values;
using System.Text.Json;

namespace Reposcraper.Scrapers
{
    public class GitHubScraper : ScraperBase
    {
        public GitHubScraper(string? personalAccessToken = null) : base(personalAccessToken) { }
        protected override void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
        }

        public override bool CanScrape(string repositoryUrl)
        {
            return repositoryUrl.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase);
        }

        public override async Task<RepositoryInfo> GetRepositoryInfoAsync(string repositoryUrl)
        {
            Uri uri = new Uri(repositoryUrl);

            string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
            {
                throw new ArgumentException("Invalid GitHub repository URL. Expected: https://github.com/owner/repoName");
            }

            string owner = segments[0];
            string repoName = segments[1];
            string apiUrl = $"https://api.github.com/repos/{owner}/{repoName}";

            JsonElement repoDataResponse = await GetJsonAsync(apiUrl);
            AssertDataCorrectness(repoName, owner, repoDataResponse);

            return new RepositoryInfo(
                repoName,
                owner,
                apiUrl + "/contents",
                repositoryUrl
            );
        }

        public override async Task<RepositoryFile[]> GetFilesAsync(RepositoryInfo repoInfo, string fileExtension)
        {
            List<RepositoryFile> files = new List<RepositoryFile>();
            await FetchFilesRecursive(repoInfo.ApiUrl, files, repoInfo, fileExtension);
            return files.ToArray();
        }

        private async Task FetchFilesRecursive(string apiUrl, List<RepositoryFile> files, RepositoryInfo repoInfo, string fileExtension)
        {
            JsonElement jsonRoot = await GetJsonAsync(apiUrl);

            foreach (JsonElement item in jsonRoot.EnumerateArray())
            {
                string type = GetStringProperty(item, "type");
                string path = GetStringProperty(item, "path");

                if (type == "file" && MatchesFileExtension(path, fileExtension))
                {
                    string name = GetStringProperty(item, "name");
                    string downloadUrl = GetStringProperty(item, "download_url");
                    files.Add(new RepositoryFile(repoInfo, name, path, downloadUrl));
                }
                else if (type == "dir")
                {
                    string dirApiUrl = $"{repoInfo.ApiUrl}/{path}";
                    await FetchFilesRecursive(dirApiUrl, files, repoInfo, fileExtension);
                }
            }
        }

        private void AssertDataCorrectness(string expectedName, string expectedOwner, JsonElement response)
        {
            string actualName = GetStringProperty(response, "name");
            string actualOwner = GetStringProperty(response.GetProperty("owner"), "login");

            if (actualName != expectedName)
            {
                throw new InvalidOperationException($"Repository name does not match expected value. Expected: {expectedName}, Actual: {actualName}");
            }

            if (actualOwner != expectedOwner)
            {
                throw new InvalidOperationException($"Repository owner does not match expected value. Expected: {expectedOwner}, Actual: {actualOwner}");
            }
        }
    }
}