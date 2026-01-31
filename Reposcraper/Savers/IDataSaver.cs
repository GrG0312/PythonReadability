using Reposcraper.Extractors.Values;
using Reposcraper.Savers.Values;

namespace Reposcraper.Savers
{
    public interface IDataSaver
    {
        public Task<SaveResult> SaveDataAsync(IExtractedData data, string outputPath);
    }
}
