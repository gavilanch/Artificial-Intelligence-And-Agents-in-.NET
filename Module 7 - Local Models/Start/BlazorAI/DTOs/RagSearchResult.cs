namespace BlazorAI.DTOs
{
    public record RagSearchResult(string DocumentTitle, string Text)
    {
        public override string ToString()
        {
            return $"""
                    Document: {DocumentTitle}
                    Content: {Text}
                    """;
        }
    }
}
