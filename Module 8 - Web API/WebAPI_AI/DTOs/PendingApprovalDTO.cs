namespace WebAPI_AI.DTOs
{
    public class PendingApprovalDTO
    {
        public string ToolName { get; set; } = string.Empty;
        public Dictionary<string, object?> Arguments { get; set; } = [];
    }
}
