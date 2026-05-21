namespace BlazorAI.Utilities
{
    public static class AIModels
    {
        private static readonly Dictionary<string, string> Models = new(StringComparer.OrdinalIgnoreCase)
        {
            ["qwen3.5:0.8b"] = "ollama",
            ["qwen3.5:2b"] = "ollama",
        };

        public static string GetProvider(string model)
        {
            if (Models.TryGetValue(model, out var provider))
            {
                return provider;
            }

            throw new ArgumentException($"Model not supported: {model}");
        }

        public static IEnumerable<string> GetAvailableModels() => Models.Keys;
        public static string GetDefaultModel => "qwen3.5:2b";

    }
}
