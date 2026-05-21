using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BlazorAI.DTOs;
using Microsoft.Extensions.AI;

namespace BlazorAI.Services.RAG
{
    public class AzureSearchRAGService : IRAGService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private SearchClient searchClient;

        public AzureSearchRAGService(IConfiguration configuration, 
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            this.embeddingGenerator = embeddingGenerator;

            var endpoint = configuration["AzureSearch:Endpoint"]!;
            var apiKey = configuration["AzureSearch:ApiKey"]!;
            var indexName = configuration["AzureSearch:IndexName"]!;

            searchClient = new SearchClient(
                              new Uri(endpoint),
                            indexName,
                            new AzureKeyCredential(apiKey));

        }

        public async Task<List<RagSearchResult>> FindRelevantContext(string prompt, int top = 3, float minScore = 0.6F, CancellationToken cancellationToken = default)
        {
            var promptEmbedding = await embeddingGenerator.GenerateVectorAsync(prompt,
                                cancellationToken: cancellationToken);

            var options = new SearchOptions
            {
                Size = top,
                Select = { nameof(DocumentRag.Id),
                nameof(DocumentRag.TitleDocument),
                nameof(DocumentRag.Text),
                nameof(DocumentRag.FragmentNumber) }
            };

            options.VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(promptEmbedding)
                    {
                        KNearestNeighborsCount = top,
                        Fields = { nameof(DocumentRag.Embedding) }
                    }
                }
            };

            var response = await searchClient.SearchAsync<DocumentRag>(null, options, cancellationToken);
            var results = new List<RagSearchResult>();

            await foreach (var item in response.Value.GetResultsAsync())
            {
                if (item.Score < minScore)
                    continue;

                results.Add(new RagSearchResult(item.Document.TitleDocument, item.Document.Text));
            }

            return results;
        }
    }
}
