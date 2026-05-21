
using Anthropic;
using FirstChatbot;
using FirstChatbot.Chatbots;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;
using System.Text;

Utilities.SetEnvironmentVariables();

// Example: dotnet run -- openai gpt-5.4-nano

//var provider = args.Length > 0 ? args[0].ToLowerInvariant() : "openai";
//var defaultModel = provider == "openai" ? "gpt-5.4-nano" : "claude-haiku-4-5";
//var model = args.Length > 1 ? args[1] : defaultModel;

//Console.WriteLine($"Using {provider} and {model}");

var provider = "ollama";
var model = "qwen3.5:2b";

var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, provider, model);
var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
await Chatbot.Run(chatClient);