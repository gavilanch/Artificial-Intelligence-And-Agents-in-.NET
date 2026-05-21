using Anthropic;
using FirstChatbot.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot
{
    internal static class Startup
    {
        public static void ConfigureServices(HostApplicationBuilder builder, string provider, string? model)
        {

            builder.Services.AddTransient<FakeGetEmailService>();
            builder.Services.AddTransient<FakeSendEmailService>();

            builder.Services.AddTransient<IWeatherService, WeatherAPIService>();
            builder.Services.AddHttpClient();
            builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);

            builder.Services.AddTransient<EvaluateWeatherConditions>();

            var keyOpenAI = Environment.GetEnvironmentVariable("OpenAIKey");
            var claudeKey = Environment.GetEnvironmentVariable("ClaudeKey");

            builder.Services.AddChatClient(sp =>
            {
                var client = provider switch
                {
                    "openai" => new OpenAI.Chat.ChatClient(model ?? "gpt-5.4-nano", keyOpenAI).AsIChatClient(),
                    "claude" => new AnthropicClient()
                    {
                        ApiKey = claudeKey
                    }.AsIChatClient()
                    .AsBuilder()
                    .ConfigureOptions(c => c.ModelId = model ?? "claude-haiku-4-5")
                    .Build(),
                    "ollama" => new OllamaApiClient("http://127.0.0.1:11434", model ?? "qwen3.5:2b"),
                    _ => throw new ArgumentException($"Unknown provider: {provider}")
                };

                return client
                .AsBuilder()
                .ConfigureOptions(o =>
                {
                    o.MaxOutputTokens = 2000;
                    o.Temperature = 0.7f;
                    o.Tools = [.. Tools.GetTools(sp)];
                })
                .UseFunctionInvocation(null, c =>
                {
                    c.IncludeDetailedErrors = true;
                })
                .Use(async (messages, options, next, cancellationToken) =>
                {
                    await next(messages, options, cancellationToken);
                })
                .Build(sp);
            });
        }
    }
}
