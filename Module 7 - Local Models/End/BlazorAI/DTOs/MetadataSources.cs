using System.Text.Json.Serialization;

namespace BlazorAI.DTOs
{
    public class MetadataSources
    {
        [JsonPropertyName("usedSources")]
        public List<string> UsedSources { get; set; } = [];

    }
}
