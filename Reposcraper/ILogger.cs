namespace Reposcraper
{
    public interface ILogger
    {
        public void Write(string message);
        public void WriteLine(string message);
    }
}
