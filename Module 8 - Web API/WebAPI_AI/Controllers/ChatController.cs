using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Security.Claims;
using System.Text.Json;
using WebAPI_AI.Data;
using WebAPI_AI.DTOs;
using WebAPI_AI.Entities;
using WebAPI_AI.Services.Chatbots;
using WebAPI_AI.Utilities;

namespace WebAPI_AI.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController(IChatClientFactory chatClientFactory,
        ChatOptions chatOptions, ApplicationDbContext context) : ControllerBase
    {

        [HttpGet("GetConversations")]
        public async Task<ActionResult<List<ConversationSummaryDTO>>> GetConversations()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var conversations = await context.Conversations
                                       .Where(x => x.UserId == userId)
                                       .OrderByDescending(x => x.UpdatedAtUtc)
                                       .Select(x => new ConversationSummaryDTO
                                       {
                                           Id = x.Id,
                                           Title = x.Title
                                       })
                                       .ToListAsync();

            return conversations;
        }

        [HttpGet("GetConversation")]
        public async Task<ActionResult<ConversationDTO>> GetConversation(Guid id)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Forbid();
            }

            var conversationDB = await context.Conversations
        .Include(x => x.Messages.OrderBy(x => x.Order))
        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (conversationDB is null)
            {
                return NotFound();
            }

            var approvalRequest = await GetPendingApprovalRequest(id, userId);

            var conversation = new ConversationDTO
            {
                Id = id,
                Messages = conversationDB.Messages
                                .Select(x => new ChatMessageUI { Role = x.Role, Text = x.Text })
                                .ToList(),
                PendingApproval = approvalRequest?.ToDTO()
            };

            return conversation;

        }

        [HttpPost("NewChat")]
        public async Task<IActionResult> NewChat()
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var id = Guid.NewGuid();

            var now = DateTime.UtcNow;

            context.Conversations.Add(new ChatConversation
            {
                Id = id,
                Title = "New chat",
                UserId = userId,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

            await context.SaveChangesAsync();

            return Ok(new { id });
        }

        [HttpPost("ResolveApproval")]
        public async Task<IActionResult> ResolveApproval([FromBody] ResolveApprovalDTO dto)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var conversationDB = await context.Conversations
       .Include(x => x.Messages.OrderBy(x => x.Order))
       .FirstOrDefaultAsync(x => x.Id == dto.ConversationId && x.UserId == userId);

            if (conversationDB is null)
            {
                return BadRequest("The conversation does not exist.");
            }

            var approvalRequest = await GetPendingApprovalRequest(dto.ConversationId, userId);

            if (approvalRequest is null)
            {
                return BadRequest("The conversation does not have an active pending approval.");
            }

            approvalRequest.Status = dto.Approved
                                        ? ChatApprovalRequestStatus.Approved
                                        : ChatApprovalRequestStatus.Rejected;

            approvalRequest.ResolvedAtUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var chatMessagesUI = conversationDB.Messages
                               .Select(x => new ChatMessageUI { Role = x.Role, Text = x.Text })
                               .ToList();

            chatMessagesUI.Add(new ChatMessageUI
            {
                Role = MessageRole.System,
                Text = dto.Approved ? "Action approved by the user" : "Action rejected by the user"
            });

            await SaveMessages(dto.ConversationId, userId, chatMessagesUI);

            var nextApprovalRequest = await GetPendingApprovalRequest(dto.ConversationId, userId);

            if (nextApprovalRequest is not null)
            {
                return Ok();
            }

            var resolvedBatch = await context.ApprovalRequests
                               .Where(x =>
                                   x.ChatConversationId == dto.ConversationId &&
                                   x.Conversation.UserId == userId &&
                                   (x.Status == ChatApprovalRequestStatus.Approved ||
                                    x.Status == ChatApprovalRequestStatus.Rejected))
                               .OrderBy(x => x.Order)
                               .ToListAsync();

            var requests = resolvedBatch
                           .Select(x => JsonSerializer.Deserialize<ToolApprovalRequestContent>(
                               x.RequestJson,
                               AIJsonUtilities.DefaultOptions))
                           .ToList();

            var responses = new List<ToolApprovalResponseContent>();

            for (int i = 0; i < requests.Count; i++)
            {
                var currentRequest = requests[i];
                var currentResolvedBatchItem = resolvedBatch[i];

                var response = currentRequest!.CreateResponse(
                      currentResolvedBatchItem.Status == ChatApprovalRequestStatus.Approved,
                      null);

                responses.Add(response);
            }

            var chatbot = await CreateChatbot(dto.ConversationId, userId);

            await chatbot.RespondToApprovalRequests(
                        requests!,
                        responses,
                        HttpContext.RequestAborted);

            foreach (var item in resolvedBatch)
            {
                item.Status = ChatApprovalRequestStatus.Completed;
            }

            await context.SaveChangesAsync();

            await SaveMessages(dto.ConversationId, userId, chatbot.Conversation);
            await PersistApprovalRequests(dto.ConversationId, chatbot.GeneratedApprovalRequests);

            return Ok();
        }

        [HttpPost("Send")]
        public async Task Send([FromBody] SendMessageDTO dto)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                await WriteStreamEvent(StreamEvent.Error, new { message = "You must sign in." });
                return;
            }

            if (dto.ConversationId is null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                await WriteStreamEvent(StreamEvent.Error, new { message = "You must provide the conversation Id." });
                return;
            }

            var id = dto.ConversationId.Value;

            var conversationExists = await context.Conversations
                        .AnyAsync(x => x.Id == id && x.UserId == userId);

            if (!conversationExists)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                await WriteStreamEvent(StreamEvent.Error, new { message = "The conversation does not exist." });
                return;
            }

            Response.ContentType = "application/x-ndjson";
            Response.Headers.CacheControl = "no-cache";

            await WriteStreamEvent(StreamEvent.StartConversation, new { id });

            var chatbot = await CreateChatbot(id, userId);

            try
            {
                await chatbot.SendMessageStreamAsync(dto.Text, async delta =>
                {
                    await WriteStreamEvent(StreamEvent.Delta, new { text = delta });
                }, HttpContext.RequestAborted);

                await SaveMessages(id, userId, chatbot.Conversation);

                if (chatbot.GeneratedApprovalRequests.Any())
                {
                    await PersistApprovalRequests(id, chatbot.GeneratedApprovalRequests);

                    var pendingApproval = await GetPendingApprovalRequest(id, userId);
                    var pendingApprovalDTO = pendingApproval!.ToDTO();

                    await WriteStreamEvent(StreamEvent.ApprovalRequired, new
                    {
                        toolName = pendingApprovalDTO.ToolName,
                        arguments = pendingApprovalDTO.Arguments
                    });
                }

            }
            catch (OperationCanceledException)
            {
                await SaveMessages(id, userId, chatbot.Conversation);
            }
        }

        [HttpPost("DeleteConversation")]
        public async Task<IActionResult> DeleteConversation([FromBody] Guid id)
        {
            var userId = GetUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var deletedConversation = await context.Conversations
                                      .Where(x => x.Id == id && x.UserId == userId)
                                      .ExecuteDeleteAsync();

            if (deletedConversation == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        private async Task SaveMessages(Guid conversationId, string userId, List<ChatMessageUI> messages)
        {
            var conversation = await context.Conversations
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.Id == conversationId && x.UserId == userId);

            if (conversation is null)
            {
                return;
            }

            var persistedMessages = conversation.Messages
                                      .OrderBy(x => x.Order)
                                      .ToList();

            for (var index = persistedMessages.Count; index < messages.Count; index++)
            {
                var message = messages[index];

                context.Messages.Add(new Entities.ChatMessage
                {
                    ChatConversationId = conversationId,
                    Role = message.Role,
                    Text = message.Text,
                    Order = index,
                });
            }

            conversation.UpdatedAtUtc = DateTime.UtcNow;
            conversation.Title = GetTitle(conversation.Title, messages);
            await context.SaveChangesAsync();
        }

        private async Task PersistApprovalRequests(
    Guid conversationId,
    List<ToolApprovalRequestContent> approvalRequests)
        {
            if (!approvalRequests.Any())
            {
                return;
            }

            var order = -1;

            var approvalRequestsExist = await context.ApprovalRequests
                                         .AnyAsync(x => x.ChatConversationId == conversationId);

            if (approvalRequestsExist)
            {
                order = await context.ApprovalRequests
                               .Where(x => x.ChatConversationId == conversationId)
                               .Select(x => x.Order)
                               .MaxAsync();
            }

            var now = DateTime.UtcNow;

            foreach (var approvalRequest in approvalRequests)
            {
                order++;

                if (approvalRequest.ToolCall is not FunctionCallContent functionCall)
                {
                    continue;
                }

                context.ApprovalRequests.Add(new ChatApprovalRequest
                {
                    ChatConversationId = conversationId,
                    Order = order,
                    Status = ChatApprovalRequestStatus.Pending,
                    ToolName = Chatbot.ConvertFunctionName(functionCall.Name),
                    ArgumentsJson = JsonSerializer.Serialize(functionCall.Arguments, AIJsonUtilities.DefaultOptions),
                    RequestJson = JsonSerializer.Serialize(approvalRequest, AIJsonUtilities.DefaultOptions),
                    CreatedAtUtc = now
                });
            }

            await context.SaveChangesAsync();
        }

        private async Task<ChatApprovalRequest?> GetPendingApprovalRequest(Guid conversationId, string userId)
        {
            return await context.ApprovalRequests
                .Where(x =>
                    x.ChatConversationId == conversationId &&
                    x.Conversation.UserId == userId &&
                    x.Status == ChatApprovalRequestStatus.Pending)
                .OrderBy(x => x.Order)
                .FirstOrDefaultAsync();
        }

        private static string GetTitle(string currentTitle, List<ChatMessageUI> messages)
        {
            if (currentTitle != "New chat")
            {
                return currentTitle;
            }

            var firstUserMessage = messages
                                   .FirstOrDefault(x => x.Role == MessageRole.User)?
                                   .Text
                                   .Trim();

            if (string.IsNullOrWhiteSpace(firstUserMessage))
            {
                return currentTitle;
            }

            return firstUserMessage.Length <= 60
                           ? firstUserMessage
                           : firstUserMessage[..60] + "...";
        }

        private async Task<Chatbot> CreateChatbot(Guid conversationId, string userId)
        {
            var messages = await context.Messages
                .Where(x => x.ChatConversationId == conversationId && x.Conversation.UserId == userId)
                .OrderBy(x => x.Order)
                .ToListAsync();

            var chatMessagesUI = messages.Select(x => new ChatMessageUI { Role = x.Role, Text = x.Text });

            return new Chatbot(chatClientFactory, chatOptions, chatMessagesUI);
        }

        private async Task WriteStreamEvent(StreamEvent type, object data)
        {
            var theEvent = JsonSerializer.Serialize(new
            {
                type = type.ToString(),
                data
            });

            await Response.WriteAsync(theEvent + "\n");
            await Response.Body.FlushAsync();
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
