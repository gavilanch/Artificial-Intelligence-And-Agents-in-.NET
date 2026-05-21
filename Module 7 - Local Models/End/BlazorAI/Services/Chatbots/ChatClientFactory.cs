using Anthropic;
using BlazorAI.Utilities;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace BlazorAI.Services.Chatbots
{
    public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
    {
        public IChatClient Create(string model)
        {
            var openAIKey = configuration.GetValue<string>("OpenAIKey");
            var claudeKey = configuration.GetValue<string>("ClaudeKey");
            var urlOllama = configuration.GetValue<string>("OLLAMA_ENDPOINT")!;

            var provider = AIModels.GetProvider(model);

            var client = provider switch
            {
                "openai" => new OpenAI.Chat.ChatClient(model ?? "gpt-5.4-nano", openAIKey).AsIChatClient(),
                "claude" => new AnthropicClient()
                {
                    ApiKey = claudeKey
                }.AsIChatClient()
                .AsBuilder()
                .ConfigureOptions(c => c.ModelId = model ?? "claude-haiku-4-5")
                .Build(),
                "ollama" => new OllamaApiClient(urlOllama, model ?? "qwen3.5:2b"),
                _ => throw new ArgumentException($"Unknown provider: {provider}")
            };

            return client
                    .AsBuilder()
                    .UseFunctionInvocation(null, c =>
                    {
                        c.IncludeDetailedErrors = true;
                    })
                    .Build(sp);
        }
    }
}
