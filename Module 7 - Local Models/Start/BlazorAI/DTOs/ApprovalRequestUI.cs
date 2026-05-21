using Microsoft.Extensions.AI;

namespace BlazorAI.DTOs
{
    public class ApprovalRequestUI
    {
        public required ToolApprovalRequestContent ApprovalRequest { get; set; }
        public required string ToolName { get; set; }
        public Dictionary<string, object?> Arguments { get; set; } = [];
    }
}
