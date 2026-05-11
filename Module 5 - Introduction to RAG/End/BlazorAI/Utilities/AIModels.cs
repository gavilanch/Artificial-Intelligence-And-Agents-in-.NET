namespace BlazorAI.Utilities
{
    public static class AIModels
    {
        private static readonly Dictionary<string, string> Models = new(StringComparer.OrdinalIgnoreCase)
        {
            ["gpt-5.4-nano"] = "openai",
            ["gpt-5.4"] = "openai",
            ["claude-haiku-4-5"] = "claude",
            ["claude-sonnet-4-5"] = "claude"
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
        public static string GetDefaultModel => "gpt-5.4-nano";

    }
}
