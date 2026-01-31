using Reposcraper.Extractors;
using Reposcraper.Extractors.PatternMatcher;
using Reposcraper.Extractors.Values;
using Reposcraper.Scrapers.Values;

namespace ReposcraperTest.ExtractorTest
{
    public class MethodExtractorTest
    {
        [Test]
        public void CanHandleTest()
        {
            MethodExtractor extractor = new MethodExtractor(new CSharpPatternMatcher());
            Assert.IsTrue(extractor.CanHandle(".cs"));
        }
    }
}
