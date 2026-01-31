using Reposcraper.Scrapers;
using Reposcraper.Scrapers.Values;

namespace ReposcraperTest.ScraperTest
{
    public class GitHubTest
    {
        private readonly string testRepoUrl = "https://github.com/GrG0312/ELTE-IK";
        private readonly GitHubScraper _gitHubScraper = new GitHubScraper(TestConfiguration.GitHubToken);

        [Test]
        public void CanScrapeTest()
        {
            bool result = _gitHubScraper.CanScrape(testRepoUrl);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task RepoInfoTest()
        {
            RepositoryInfo repoInfo = await _gitHubScraper.GetRepositoryInfoAsync(testRepoUrl);
            Assert.IsNotNull(repoInfo);
            Assert.That(repoInfo.RepoName, Is.EqualTo("ELTE-IK"));
            Assert.That(repoInfo.Owner, Is.EqualTo("GrG0312"));
        }

        [Test]
        public async Task GetFilesTest()
        {
            RepositoryInfo repoInfo = await _gitHubScraper.GetRepositoryInfoAsync(testRepoUrl);
            RepositoryFile[] files = await _gitHubScraper.GetFilesAsync(repoInfo, "py");
            Assert.IsNotNull(files);
            Assert.IsNotEmpty(files);
            RepositoryFile? netcopy = files.SingleOrDefault(f => f.Name == "netcopy_cli.py");
            RepositoryFile? server3 = files.SingleOrDefault(f => f.Name == "server_3.py");
            RepositoryFile? proxy = files.SingleOrDefault(f => f.Name == "hibas_proxy.py");
            Assert.IsNotNull(netcopy);
            Assert.IsNotNull(server3);
            Assert.IsNotNull(proxy);
            Assert.IsTrue(netcopy.Name == "netcopy_cli.py");
            Assert.IsTrue(server3.Name == "server_3.py");
            Assert.IsTrue(proxy.Name == "hibas_proxy.py");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _gitHubScraper.Dispose();
        }
    }
}
