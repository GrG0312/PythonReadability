using Microsoft.VisualBasic.FileIO;

namespace CodeReadabilityLib.Datasets.Loaders
{
    public sealed class CsvDatasetLoader : IDatasetLoader
    {
        public List<CodeSnippetSample> Load(string path)
        {
            List<CodeSnippetSample> samples = new List<CodeSnippetSample>();

            using TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            string[]? header = parser.ReadFields();
            if (header is null)
            {
                return [];
            }

            int idIndex = Find(header, "id");
            int codeIndex = Find(header, "code");
            int scoreIndex = Find(header, "score");

            if (codeIndex < 0 || scoreIndex < 0)
            {
                throw new InvalidOperationException("CSV must contain at least 'code' and 'score' columns.");
            }

            int row = 0;
            while (!parser.EndOfData)
            {
                row++;
                string[]? fields = parser.ReadFields();
                if (fields is null) continue;

                string code = Get(fields, codeIndex);
                if (string.IsNullOrWhiteSpace(code)) continue;

                string id = idIndex >= 0 ? Get(fields, idIndex) : $"row-{row}";
                int goldScore = Clamp1To5(int.Parse(Get(fields, scoreIndex)));

                samples.Add(new CodeSnippetSample(id, code, goldScore));
            }

            return samples;
        }

        private static int Find(string[] header, string name)
        {
            return Array.FindIndex(header, h => string.Equals(h.Trim(), name, StringComparison.OrdinalIgnoreCase));
        }

        private static string Get(string[] fields, int index)
        {
            if (index >= 0 && index < fields.Length)
            {
                return fields[index];
            }
            return string.Empty;
        }

        private static int Clamp1To5(int score)
        {
            return Math.Min(5, Math.Max(1, score));
        }
    }
}
