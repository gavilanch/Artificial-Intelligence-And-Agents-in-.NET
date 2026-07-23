using Microsoft.Extensions.AI;
using System.Text.Json;
using WebAPI_AI.DTOs;
using WebAPI_AI.Entities;

namespace WebAPI_AI.Utilities
{
    public static class ExtensionMethods
    {
        public static PendingApprovalDTO ToDTO(this ChatApprovalRequest approvalRequest)
        {
            return new PendingApprovalDTO
            {
                ToolName = approvalRequest.ToolName,
                Arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                                approvalRequest.ArgumentsJson,
                                AIJsonUtilities.DefaultOptions) ?? []
            };
        }
    }
}
