using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot.Chatbots
{
    internal class Chatbot
    {
        public static async Task Run(IChatClient client)
        {
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


            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));


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

                messages.Add(new ChatMessage(ChatRole.User, usersPrompt));

                Console.WriteLine();


                Console.Write("AI: ");

                var sb = new StringBuilder();

                await foreach (var fragment in client.GetStreamingResponseAsync(messages))
                {
                    Console.Write(fragment);
                    sb.Append(fragment);
                }

                messages.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
