using Microsoft.Extensions.AI;

namespace WebAPI_AI.Services.Chatbots
{
    public interface IChatClientFactory
    {
        IChatClient Create();
    }
}
