using CodeReadabilityLib.Datasets;
using CodeReadabilityLib.Evaluation;
using CodeReadabilityLib.Judger;

namespace CodeReadabilityLib.Evaluators
{
    public sealed class ModelRankingService
    {
        private readonly int _port;
        private readonly int _gpuLayers;

        public ModelRankingService(int port, int gpuLayers)
        {
            _port = port;
            _gpuLayers = gpuLayers;
        }

        public async Task<EvaluationReport> EvaluateAsync(
            string datasetPath,
            string ggufDir,
            int minScore,
            int maxScore,
            CancellationToken ctoken = default)
        {
            // 1. Load Dataset
            IDatasetLoader loader = DatasetLoaderFactory.Create(datasetPath);
            List<CodeSnippetSample> samples = loader.Load(datasetPath);

            // 2. Discover Models
            string[] ggufFiles = Directory.GetFiles(ggufDir, "*.gguf", SearchOption.TopDirectoryOnly);

            if (ggufFiles.Length == 0)
            {
                throw new FileNotFoundException($"No .gguf files found in directory: {ggufDir}");
            }

            EvaluationReport report = new EvaluationReport { TotalSnippets = samples.Count };

            // 3. Evaluate each model
            foreach (string ggufPath in ggufFiles)
            {
                ModelResult modelResult = new ModelResult
                {
                    ModelName = Path.GetFileNameWithoutExtension(ggufPath)
                };

                using RangeGgufJudge judge = new RangeGgufJudge(ggufPath, _port, _gpuLayers, minScore, maxScore);
                await judge.StartAsync();

                foreach (CodeSnippetSample sample in samples)
                {
                    if (ctoken.IsCancellationRequested) break;

                    int predicted = await judge.ScoreSnippetAsync(sample.Code, ctoken);
                    int error = Math.Abs(predicted - sample.Score);

                    // Sort into the 4 buckets array: exactly 0, off by 1, off by 2, or >= 3
                    int bucketIndex = Math.Min(3, error);
                    modelResult.Answers[bucketIndex]++;

                    modelResult.ConcreteAnswers.Add(new ConcreteAnswer
                    {
                        SnippetId = sample.Id,
                        OriginalScore = sample.Score,
                        ModelAnswer = predicted
                    });
                }

                report.ModelResults.Add(modelResult);
            }

            // 4. Sort models (Descending Exact, then Desc. Off-by-1, then Desc. Off-by-2, then Desc. Off-by-3+)
            report.ModelResults = report.ModelResults
                .OrderByDescending(m => m.Answers[0])
                .ThenByDescending(m => m.Answers[1])
                .ThenByDescending(m => m.Answers[2])
                .ThenByDescending(m => m.Answers[3])
                .ToList();

            return report;
        }
    }
}
