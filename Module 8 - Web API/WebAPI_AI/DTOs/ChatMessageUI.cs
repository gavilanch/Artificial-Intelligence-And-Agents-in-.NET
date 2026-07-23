namespace WebAPI_AI.DTOs
{
    public class ChatMessageUI
    {
        public MessageRole Role { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public enum MessageRole
    {
        User, AI, System
    }

}
