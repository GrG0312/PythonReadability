using CodeReadabilityLib.Datasets.Loaders;

namespace CodeReadabilityLib.Datasets
{
    public static class DatasetLoaderFactory
    {
        public static IDatasetLoader Create(string datasetType)
        {
            return datasetType switch
            {
                ".csv" => new CsvDatasetLoader(),
                _ => throw new ArgumentException($"Unsupported dataset type: {datasetType}")
            };
        }
    }
}
