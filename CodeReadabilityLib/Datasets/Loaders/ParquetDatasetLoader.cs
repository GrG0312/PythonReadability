using ParquetSharp;

namespace CodeReadabilityLib.Datasets.Loaders
{
    public sealed class ParquetDatasetLoader : IDatasetLoader
    {
        // To maintain compatibility with the interface
        public List<CodeSnippetSample> Load(string path)
        {
            return LoadAsync(path).GetAwaiter().GetResult();
        }

        private async Task<List<CodeSnippetSample>> LoadAsync(string path)
        {
            List<CodeSnippetSample> samples = new List<CodeSnippetSample>();
            using ParquetFileReader reader = new ParquetFileReader(path);

            for (int rowGroup = 0; rowGroup < reader.FileMetaData.NumRowGroups; rowGroup++)
            {
                using RowGroupReader rowGroupReader = reader.RowGroup(rowGroup);

                int numberOfRowsInGroup = checked((int)rowGroupReader.MetaData.NumRows);

                string[] snippets = rowGroupReader.Column(0).LogicalReader<string>().ReadAll(numberOfRowsInGroup);
                double[] scores = rowGroupReader.Column(1).LogicalReader<double>().ReadAll(numberOfRowsInGroup);

                if (snippets.Length != scores.Length)
                {
                    throw new InvalidDataException($"Mismatch in number of snippets and scores in row group {rowGroup}");
                }

                for (int i = 0; i < snippets.Length; i++)
                {
                    samples.Add(new CodeSnippetSample(Guid.NewGuid().ToString(), snippets[i], scores[i]));
                }
            }

            return samples;
        }
    }
}
