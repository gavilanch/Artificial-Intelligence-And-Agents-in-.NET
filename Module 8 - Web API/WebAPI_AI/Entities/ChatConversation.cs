using Microsoft.AspNetCore.Identity;

namespace WebAPI_AI.Entities
{
    public class ChatConversation
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "New chat";
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public List<ChatMessage> Messages { get; set; } = [];
        public List<ChatApprovalRequest> ApprovalRequests { get; set; } = [];
    }
}
