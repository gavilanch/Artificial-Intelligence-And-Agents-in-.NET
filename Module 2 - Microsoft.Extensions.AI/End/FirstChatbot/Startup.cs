using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot
{
    internal static class Startup
    {
        public static void ConfigureServices(HostApplicationBuilder builder, string provider, string? model)
        {
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
                    _ => throw new ArgumentException($"Unknown provider: {provider}")
                };

                return client
                .AsBuilder()
                .ConfigureOptions(o =>
                {
                    o.MaxOutputTokens = 100;
                })
                .Use(async (messages, options, next, cancellationToken) =>
                {
                    //Console.WriteLine();
                    //Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine("Before sending the messages to the model...");
                    //Console.ResetColor();

                    await next(messages, options, cancellationToken);
                    
                    //Console.WriteLine();
                    //Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine("After sending the messages to the model...");
                    //Console.ResetColor();
                })
                .Build(sp);
            });
        }
    }
}
