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

    If a tool fails, read the exception message to see if you can fix it by making any adjustments. Inform the user of any adjustments you make.
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

                while (true)
                {
                    var updates = new List<ChatResponseUpdate>();

                    await foreach (var fragment in client.GetStreamingResponseAsync(messages))
                    {
                        updates.Add(fragment);

                        foreach (var content in fragment.Contents)
                        {
                            if (content is TextContent textContent)
                            {
                                Console.Write(textContent.Text);
                            }
                        }
                    }

                    var response = updates.ToChatResponse();

                    messages.AddMessages(response);

                    var approvalRequest = response.Messages
                        .SelectMany(m => m.Contents)
                        .OfType<ToolApprovalRequestContent>()
                        .FirstOrDefault();

                    if (approvalRequest is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("The AI requires permission to do something.");

                        if (approvalRequest.ToolCall is FunctionCallContent functionCall)
                        {
                            Console.WriteLine($"Tool: {ConvertToolName(functionCall.Name)}");

                            if (functionCall.Arguments is not null)
                            {
                                foreach (var argument in functionCall.Arguments)
                                {
                                    Console.WriteLine($"{argument.Key}: {argument.Value}");
                                }
                            }
                        }

                        Console.ResetColor();
                        Console.Write("Do you approve this action? (y/n): ");
                        var approved = Console.ReadLine()?.Trim().ToLower() == "y";
                        var approvalResponse = approvalRequest.CreateResponse(approved);
                        messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));

                        Console.WriteLine();
                        Console.Write("AI: ");
                        continue;
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                    break;
                }
            }
        }

        private static string ConvertToolName(string toolName)
        {
            return toolName switch
            {
                "SendEmail" => "Send email",
                _ => toolName
            };
        }
    }
}
