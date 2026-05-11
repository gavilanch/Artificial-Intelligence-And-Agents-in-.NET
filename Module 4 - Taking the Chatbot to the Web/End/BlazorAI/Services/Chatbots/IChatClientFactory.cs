using Microsoft.Extensions.AI;

namespace BlazorAI.Services.Chatbots
{
    public interface IChatClientFactory
    {
        IChatClient Create(string model);
    }
}
