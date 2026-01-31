namespace Reposcraper.Judger
{
    /// <summary>
    /// Defines a contract for evaluating the readability of text content.
    /// </summary>
    public interface IReadabilityJudge
    {
        /// <summary>
        /// Asynchronously evaluates the readability of the specified content and returns a list of readability scores.
        /// </summary>
        /// <param name="request">The request containing the content and evaluation parameters to be analyzed. Cannot be null.</param>
        /// <param name="ctoken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of readability
        /// scores for the evaluated content.</returns>
        public Task<IReadOnlyList<ReadabilityScore>> EvaluateAsync(ReadabilityEvaluationRequest request, CancellationToken ctoken = default);
    }
}
