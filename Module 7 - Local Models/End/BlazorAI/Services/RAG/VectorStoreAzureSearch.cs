using Azure;
using Azure.Search.Documents;
using BlazorAI.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.AI;

namespace BlazorAI.Services.RAG
{
    public class VectorStoreAzureSearch : IVectorStore
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private readonly IndexConfigurationAzureSearchService indexConfigurationAzureSearchService;
        private SearchClient searchClient;

        public VectorStoreAzureSearch(IConfiguration configuration,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IndexConfigurationAzureSearchService indexConfigurationAzureSearchService)
        {
            this.embeddingGenerator = embeddingGenerator;
            this.indexConfigurationAzureSearchService = indexConfigurationAzureSearchService;

            var endpoint = configuration["AzureSearch:Endpoint"]!;
            var apiKey = configuration["AzureSearch:ApiKey"]!;
            var indexName = configuration["AzureSearch:IndexName"]!;


            searchClient = new SearchClient(
                new Uri(endpoint),
                indexName,
                new AzureKeyCredential(apiKey));

        }

        public async Task UploadFiles(List<IBrowserFile> files, CancellationToken cancellationToken = default)
        {
            await indexConfigurationAzureSearchService.CreateIfNotExists(cancellationToken);

            var documents = new List<DocumentRag>();

            foreach (var file in files)
            {
                using var reader = new StreamReader(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));

                var content = await reader.ReadToEndAsync(cancellationToken);

                var fragments = SplitIntoChunks(content, maxCharacters: 1200);

                for (int i = 0; i < fragments.Count; i++)
                {
                    var embedding = await embeddingGenerator.GenerateVectorAsync(fragments[i],
                                                cancellationToken: cancellationToken);

                    var validName = Path.GetFileNameWithoutExtension(file.Name).Replace(" ", "-");

                    documents.Add(new DocumentRag
                    {
                        Id = $"{validName}-{i}-{Guid.NewGuid()}",
                        TitleDocument = file.Name,
                        Text = fragments[i],
                        FragmentNumber = i,
                        Embedding = embedding.ToArray()
                    });
                }
            }

            if (documents.Count > 0)
            {
                await searchClient.UploadDocumentsAsync(documents);
            }
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
                }
                else
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
