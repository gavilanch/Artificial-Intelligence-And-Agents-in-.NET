namespace WebAPI_AI.DTOs
{
    public class ConversationDTO
    {
        public Guid Id { get; set; }
        public List<ChatMessageUI> Messages { get; set; } = [];
        public PendingApprovalDTO? PendingApproval { get; set; }
    }
}
