using Reposcraper.Scrapers.Values;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Reposcraper.Scrapers
{
    public abstract class ScraperBase : IRepositoryScraper, IDisposable
    {
        protected readonly HttpClient _httpClient;
        private readonly string _personalAccessToken;

        protected ScraperBase(string? personalAccessToken = null)
        {
            _personalAccessToken = personalAccessToken ?? string.Empty;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"ReposcraperBot/{Reposcraper.Version}");
            if (!string.IsNullOrEmpty(_personalAccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _personalAccessToken);
            }
            ConfigureHttpClient();
        }

        /// <summary>
        /// Configure HTTP client with provider-specific headers and settings
        /// </summary>
        protected virtual void ConfigureHttpClient()
        {
            // Default implementation - derived classes can override for specific configurations
        }

        /// <summary>
        /// Make HTTP GET request and return JSON response
        /// </summary>
        /// <param name="url">The URL to request</param>
        /// <returns>JsonElement representing the response</returns>
        protected async Task<JsonElement> GetJsonAsync(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json).RootElement;
        }

        /// <summary>
        /// Check if a file matches the specified extension
        /// </summary>
        /// <param name="fileName">The file name or path</param>
        /// <param name="fileExtension">The extension to match</param>
        /// <returns>True if the file matches the extension</returns>
        protected bool MatchesFileExtension(string fileName, string fileExtension)
        {
            return fileName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Safely get string property from JsonElement
        /// </summary>
        /// <param name="element">The JsonElement</param>
        /// <param name="propertyName">The property name</param>
        /// <returns>String value or empty string if not found</returns>
        protected string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                return property.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        public abstract bool CanScrape(string repositoryUrl);
        public abstract Task<RepositoryInfo> GetRepositoryInfoAsync(string repositoryUrl);
        public abstract Task<RepositoryFile[]> GetFilesAsync(RepositoryInfo repoInfo, string fileExtension);


        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
