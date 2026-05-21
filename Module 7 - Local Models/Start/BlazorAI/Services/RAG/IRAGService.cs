using BlazorAI.DTOs;

namespace BlazorAI.Services.RAG
{
    public interface IRAGService
    {
        Task<List<RagSearchResult>> FindRelevantContext(string prompt, int top = 3, float minScore = 0.6f, CancellationToken cancellationToken = default);

    }
}
