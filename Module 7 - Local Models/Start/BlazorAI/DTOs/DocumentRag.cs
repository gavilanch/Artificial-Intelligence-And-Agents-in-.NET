using Azure.Search.Documents.Indexes;

namespace BlazorAI.DTOs
{
    public class DocumentRag
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; } = null!;

        [SearchableField(IsFilterable = true)]
        public string TitleDocument { get; set; } = null!;

        [SearchableField]
        public string Text { get; set; } = null!;

        [SimpleField(IsFilterable = true)]
        public int FragmentNumber { get; set; }

        [VectorSearchField(VectorSearchDimensions = 1536, VectorSearchProfileName = "vector-profile")]
        public float[] Embedding { get; set; } = null!;
    }
}
