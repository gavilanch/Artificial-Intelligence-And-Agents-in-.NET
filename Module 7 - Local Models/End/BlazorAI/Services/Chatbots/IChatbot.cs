using BlazorAI.DTOs;

namespace BlazorAI.Services.Chatbots;

public interface IChatbot
{
    List<ChatMessageUI> Conversation { get; }
    bool IsProcessing { get; }
    ApprovalRequestUI? PendingApproval { get; }

    event Action? OnChange;

    void CancelCurrentResponse();
    Task SendMessageAsync(string userText, CancellationToken cancellationToken = default);
    Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default);
    void SetModel(string model);
}
