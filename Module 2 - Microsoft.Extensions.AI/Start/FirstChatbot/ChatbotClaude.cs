using Anthropic;
using Anthropic.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace FirstChatbot
{
    internal class ChatbotClaude
    {
        public static async Task Run()
        {
            var key = Environment.GetEnvironmentVariable("ClaudeKey");

            var client = new AnthropicClient
            {
                ApiKey = key
            };

            var model = "claude-haiku-4-5";

            Console.WriteLine("AI: Hello! You can ask any questions or press Enter to exit");
            Console.WriteLine();

            var messages = new List<MessageParam>();

            var systemPrompt = """
                                You are an assistant who answers general questions. 
                                You must answer in English. 
                                Answers must be in plain text; do not use formatting such as Markdown.
                                """;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("You: ");
                var usersPrompt = Console.ReadLine();
                Console.ResetColor();

                if (string.IsNullOrEmpty(usersPrompt))
                {
                    break;
                }

                messages.Add(new MessageParam
                {
                    Role = Role.User,
                    Content = usersPrompt
                });

                Console.WriteLine();
                Console.Write("AI: ");

                var parameters = new MessageCreateParams
                {
                    Model = model,
                    MaxTokens = 1000,
                    System = systemPrompt,
                    Messages = messages
                };

                var sb = new StringBuilder();

                await foreach (var chunk in client.Messages.CreateStreaming(parameters))
                {
                    var text = ExtractDeltaText(chunk);

                    if (!string.IsNullOrEmpty(text))
                    {
                        Console.Write(text);
                        sb.Append(text);
                    }
                }

                messages.Add(new MessageParam
                {
                    Role = Role.Assistant,
                    Content = sb.ToString()
                });

                Console.WriteLine();
                Console.WriteLine();

            }
        }

        private static string? ExtractDeltaText(object? chunk)
        {
            var json = chunk?.ToString();

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeProp) ||
                typeProp.GetString() != "content_block_delta")
                {
                    return null;
                }

                if (!root.TryGetProperty("delta", out var deltaProp))
                {
                    return null;
                }

                if (!deltaProp.TryGetProperty("type", out var deltaTypeProp) ||
                deltaTypeProp.GetString() != "text_delta")
                {
                    return null;
                }

                if (!deltaProp.TryGetProperty("text", out var textProp))
                {
                    return null;
                }

                return textProp.GetString();
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
