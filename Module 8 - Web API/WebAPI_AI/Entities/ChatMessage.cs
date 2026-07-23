using WebAPI_AI.DTOs;

namespace WebAPI_AI.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid ChatConversationId { get; set; }
        public ChatConversation Conversation { get; set; } = null!;
        public MessageRole Role { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
