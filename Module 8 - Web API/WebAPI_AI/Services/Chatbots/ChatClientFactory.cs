using Microsoft.Extensions.AI;

namespace WebAPI_AI.Services.Chatbots
{
    public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
    {
        public IChatClient Create()
        {
            var OPENAI_KEY = configuration.GetValue<string>("OPENAI_KEY");
            var model = configuration.GetValue<string>("OPENAI_MODEL");

            var client = new OpenAI.Chat.ChatClient(model ?? "gpt-5.4-nano", OPENAI_KEY).AsIChatClient();

            return client.AsBuilder()
            .UseFunctionInvocation(null, c =>
            {
                c.IncludeDetailedErrors = true;
            })
            .Build(sp);

        }
    }
}
