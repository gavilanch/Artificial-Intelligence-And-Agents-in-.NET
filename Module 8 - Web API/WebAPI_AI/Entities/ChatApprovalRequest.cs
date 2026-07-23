namespace WebAPI_AI.Entities
{
    public class ChatApprovalRequest
    {
        public int Id { get; set; }
        public Guid ChatConversationId { get; set; }
        public ChatConversation Conversation { get; set; } = null!;
        public int Order { get; set; }
        public ChatApprovalRequestStatus Status { get; set; }
        public string ToolName { get; set; } = string.Empty;
        public string ArgumentsJson { get; set; } = "{}";
        public string RequestJson { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ResolvedAtUtc { get; set; }
    }

    public enum ChatApprovalRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed
    }
}
