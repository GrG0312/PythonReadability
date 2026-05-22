using System.Text.Json;

namespace CodeReadabilityLib.Datasets.Loaders
{
    public sealed class JsonDatasetLoader : IDatasetLoader
    {
        private sealed class JsonRow
        {
            public string? Id { get; set; }
            public string? Code { get; set; }
            public int Score { get; set; }
        }

        public List<CodeSnippetSample> Load(string path)
        {
            string json = File.ReadAllText(path);
            JsonRow[]? rows = JsonSerializer.Deserialize<JsonRow[]>(json);

            if (rows is null || rows.Length == 0)
            {
                return [];
            }

            return rows
                .Where(row => !string.IsNullOrWhiteSpace(row.Code))
                .Select((JsonRow row, int index) =>
                    new CodeSnippetSample(
                        string.IsNullOrWhiteSpace(row.Id) ? $"Row{index + 1}" : row.Id,
                        row.Code!, // Code cannot be null because of Where
                        Math.Clamp(row.Score, 1, 5))).ToList();
        }
    }
}
