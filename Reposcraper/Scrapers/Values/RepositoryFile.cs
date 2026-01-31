namespace Reposcraper.Scrapers.Values
{
    public record RepositoryFile
    {
        public readonly RepositoryInfo HomeRepo;
        public readonly string Name;
        public readonly string Path;
        public readonly string DownloadURL;

        public RepositoryFile(RepositoryInfo homeRepo, string name, string path, string downloadUrl)
        {
            HomeRepo = homeRepo;
            Name = name;
            Path = path;
            DownloadURL = downloadUrl;
        }

        /// <summary>
        /// Gets the file extension from the file name.
        /// </summary>
        /// <returns>The file extension, including the leading dot (e.g., ".cs" or ".py").</returns>
        public string GetExtension()
        {
            int lastDotIndex = Name.LastIndexOf('.');
            if (lastDotIndex == -1 || lastDotIndex == Name.Length - 1)
            {
                return string.Empty;
            }
            return Name.Substring(lastDotIndex);
        }
    }
}
