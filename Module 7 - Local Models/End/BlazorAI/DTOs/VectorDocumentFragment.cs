using Microsoft.Extensions.VectorData;

namespace BlazorAI.DTOs
{
    public class VectorDocumentFragment
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string DocumentTitle { get; set; } = string.Empty;

        [VectorStoreData(IsFullTextIndexed = true)]
        public string Text { get; set; } = string.Empty;

        [VectorStoreVector(
                Dimensions: 1536,
                DistanceFunction = DistanceFunction.CosineSimilarity)]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
