using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot
{
    internal class ChatbotOpenAI
    {
        public static async Task Run()
        {
            var key = Environment.GetEnvironmentVariable("OpenAIKey");
            var model = "gpt-5.4-nano";
            var client = new ChatClient(model, key);

            Console.WriteLine("AI: Hello! You can ask any questions or press Enter to exit");
            Console.WriteLine();

            var messages = new List<ChatMessage>();

            var systemPrompt = """
    You are an assistant who answers general questions. 
    You must answer in English. 
    Answers must be in plain text; do not use formatting such as Markdown.
    """;

            //var systemPrompt = """
            //    You are an expert assistant in C# and .NET. 
            //    You must answer in English and provide examples. 
            //    Answers must be in plain text; do not use formatting such as Markdown.
            //    """;

            //var systemPrompt = """
            //    You are an expert assistant in Python. 
            //    You must answer in English and provide examples. 
            //    Answers must be in plain text; do not use formatting such as Markdown.
            //    """;


            messages.Add(new SystemChatMessage(systemPrompt));


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

                messages.Add(new UserChatMessage(usersPrompt));

                Console.WriteLine();

                var stream = client.CompleteChatStreamingAsync(messages);

                Console.Write("AI: ");

                var sb = new StringBuilder();

                await foreach (var update in stream)
                {
                    foreach (var content in update.ContentUpdate)
                    {
                        sb.Append(content.Text);
                        Console.Write(content.Text);
                    }
                }

                messages.Add(new AssistantChatMessage(sb.ToString()));

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
