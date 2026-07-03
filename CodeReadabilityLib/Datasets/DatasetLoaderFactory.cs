using CodeReadabilityLib.Datasets.Loaders;

namespace CodeReadabilityLib.Datasets
{
    public static class DatasetLoaderFactory
    {
        public static IDatasetLoader Create(string extension)
        {
            return extension switch
            {
                ".csv" or "csv" => new CsvDatasetLoader(),
                ".parquet" or "parquet" => new ParquetDatasetLoader(),
                _ => throw new ArgumentException($"Unsupported dataset type: {extension}")
            };
        }
    }
}
