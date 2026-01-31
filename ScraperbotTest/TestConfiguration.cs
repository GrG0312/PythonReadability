using Microsoft.Extensions.Configuration;
using ReposcraperTest.ScraperTest;

namespace ReposcraperTest
{
    internal static class TestConfiguration
    {
        private static readonly Lazy<IConfiguration> _configuration = new Lazy<IConfiguration>(BuildConfiguration);

        public static IConfiguration Configuration => _configuration.Value;

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<GitHubTest>()
                .Build();
        }

        internal static string GitHubToken => Configuration["GithubToken"] ?? string.Empty;
        internal static string HuggingFaceToken => Configuration["HuggingFaceToken"] ?? string.Empty;
    }
}
