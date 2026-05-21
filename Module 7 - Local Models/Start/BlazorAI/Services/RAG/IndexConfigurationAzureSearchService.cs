using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using BlazorAI.DTOs;

namespace BlazorAI.Services.RAG
{
    public class IndexConfigurationAzureSearchService
    {
        private string indexName;
        private SearchIndexClient indexClient;

        public IndexConfigurationAzureSearchService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureSearch:Endpoint"]!;
            var apiKey = configuration["AzureSearch:ApiKey"]!;
            indexName = configuration["AzureSearch:IndexName"]!;

            indexClient = new SearchIndexClient(
                            new Uri(endpoint),
                            new AzureKeyCredential(apiKey));

        }

        public async Task CreateIfNotExists(CancellationToken cancellationToken = default)
        {
            var indexNames = await indexClient.GetIndexNamesAsync(cancellationToken).ToListAsync(cancellationToken);

            if (indexNames.Contains(indexName, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            var fields = new FieldBuilder().Build(typeof(DocumentRag));

            var vectorSearch = new VectorSearch
            {
                Profiles =
                {
                    new VectorSearchProfile("vector-profile", "hnsw")
                },
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("hnsw")
                }
            };

            var index = new SearchIndex(indexName)
            {
                Fields = fields,
                VectorSearch = vectorSearch
            };

            await indexClient.CreateIndexAsync(index, cancellationToken);
        }
    }
}
