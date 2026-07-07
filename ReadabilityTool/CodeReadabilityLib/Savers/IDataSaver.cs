using CodeReadabilityLib.Extractors.Values;
using CodeReadabilityLib.Savers.Values;

namespace CodeReadabilityLib.Savers
{
    public interface IDataSaver
    {
        public Task<SaveResult> SaveDataAsync(IExtractedData data, string outputPath);

        public Task SaveAllResultsAsync(Dictionary<IExtractedData, List<ReadabilityScore>> results, string outputPath);
    }
}
