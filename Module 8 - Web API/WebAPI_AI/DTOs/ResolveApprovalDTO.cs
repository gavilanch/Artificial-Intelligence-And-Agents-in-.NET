namespace WebAPI_AI.DTOs
{
    public class ResolveApprovalDTO
    {
        public Guid ConversationId { get; set; }
        public bool Approved { get; set; }
    }
}
