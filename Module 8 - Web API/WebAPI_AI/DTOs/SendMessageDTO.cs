namespace WebAPI_AI.DTOs
{
    public class SendMessageDTO
    {
        public Guid? ConversationId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
