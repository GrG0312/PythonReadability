namespace CodeReadabilityLib.Datasets
{
    public interface IDatasetLoader
    {
        public List<CodeSnippetSample> Load(string path);
    }
}
