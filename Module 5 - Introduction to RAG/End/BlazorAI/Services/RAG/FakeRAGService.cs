using BlazorAI.DTOs;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;

namespace BlazorAI.Services.RAG
{
    public class FakeRAGService : IRAGService
    {
        private readonly DocumentsFromMemoryService documentsFromMemoryService;
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private readonly InMemoryCollection<Guid, VectorDocumentFragment> collection;
        private bool initialized = false;

        public FakeRAGService(DocumentsFromMemoryService documentsFromMemoryService,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            InMemoryVectorStore vectorStore)
        {
            this.documentsFromMemoryService = documentsFromMemoryService;
            this.embeddingGenerator = embeddingGenerator;

            collection = vectorStore.GetCollection<Guid, VectorDocumentFragment>("documents");
        }


        public async Task<List<RagSearchResult>> FindRelevantContext(string prompt, int top = 3, 
            float minScore = 0.6f,
            CancellationToken cancellationToken = default)
        {
            await Initialize(cancellationToken);

            var promptEmbedding = await embeddingGenerator.GenerateVectorAsync(prompt,
                cancellationToken: cancellationToken);

            var results = new List<RagSearchResult>();

            await foreach (var result in collection.SearchAsync(promptEmbedding, 
                    top: top, cancellationToken: cancellationToken))
            {
                if (result.Score < minScore)
                    continue;

                results.Add(new RagSearchResult(result.Record.DocumentTitle, result.Record.Text));
            }

            return results;
        }

        private async Task Initialize(CancellationToken cancellationToken = default)
        {
            if (initialized)
            {
                return;
            }

            await collection.EnsureCollectionExistsAsync(cancellationToken);

            var documents = documentsFromMemoryService.GetDocuments();

            foreach (var document in documents)
            {
                var chunks = SplitIntoChunks(document.Content, maxCharacters: 220);

                foreach (var chunk in chunks)
                {
                    var vector = await embeddingGenerator.GenerateVectorAsync(
                                        chunk,
                                        cancellationToken: cancellationToken);

                    var record = new VectorDocumentFragment
                    {
                        Id = Guid.NewGuid(),
                        DocumentTitle = document.Title,
                        Text = chunk,
                        Embedding = vector
                    };

                    await collection.UpsertAsync(record, cancellationToken);

                }
            }

            initialized = true;
        }

        private List<string> SplitIntoChunks(string text, int maxCharacters)
        {
            var paragraphs = text
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var result = new List<string>();
            var current = string.Empty;

            foreach (var paragraph in paragraphs)
            {
                var candidate = string.IsNullOrWhiteSpace(current)
                      ? paragraph
                      : current + "\n" + paragraph;

                if (candidate.Length > maxCharacters)
                {
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        result.Add(current);
                    }

                    current = paragraph;
                } else
                {
                    current = candidate;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                result.Add(current);
            }

            return result;
        }

    }
}
