namespace BlazorAI.DTOs;

public class ChatMessageUI
{
    public MessageRole Role { get; set; } 
    public string Text { get; set; } = string.Empty;
    public List<CitedFile> CitedFiles { get; set; } = [];
}
