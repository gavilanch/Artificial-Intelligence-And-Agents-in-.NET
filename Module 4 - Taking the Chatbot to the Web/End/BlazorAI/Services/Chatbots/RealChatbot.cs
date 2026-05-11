using BlazorAI.DTOs;
using BlazorAI.Utilities;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;

namespace BlazorAI.Services.Chatbots
{
    public class RealChatbot : IChatbot
    {
        private readonly List<ChatMessage> messages = [];
        private readonly IChatClientFactory chatClientFactory;
        private readonly ChatOptions chatOptions;
        private CancellationTokenSource? _cts;
        private string model;

        private readonly Queue<ToolApprovalRequestContent> pendingApprovals = new();

        public List<ChatMessageUI> Conversation { get; } = [];

        public RealChatbot(IChatClientFactory chatClientFactory, ChatOptions chatOptions)
        {
            model = AIModels.GetDefaultModel;
            this.chatClientFactory = chatClientFactory;
            this.chatOptions = chatOptions;
            var systemPrompt = """
                    You are an assistant that answers general questions.
                    You must respond in English.
                    Responses should be concise unless instructed otherwise.
                    Responses must be in plain text; do not use formats such as Markdown.
                    If a tool fails, read the exception message to see if you can fix it by making an adjustment. Inform the user of any adjustment you are going to make.
                    """;

            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        }

        public bool IsProcessing { get; private set; }

        public ApprovalRequestUI? PendingApproval { get; private set; }

        public event Action? OnChange;

        public void CancelCurrentResponse()
        {
            if (IsProcessing)
            {
                _cts?.Cancel();
            }
        }

        public async Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            if (PendingApproval is null || IsProcessing)
            {
                return;
            }

            try
            {
                IsProcessing = true;
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var approvalResponse = PendingApproval.ApprovalRequest.CreateResponse(approved);
                messages.Add(new ChatMessage(ChatRole.User, [approvalResponse]));

                PendingApproval = null;

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.System,
                    Text = approved ? "Action approved by the user" : "Action rejected by the user"
                });

                DisplayNextApprovalRequest();

                if (PendingApproval is not null)
                {
                    IsProcessing = false;
                    NotifyChange();
                    return;
                }

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.AI,
                    Text = string.Empty
                });

                NotifyChange();
                await SendMessagesToTheAssistant(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                HandleOperationCanceled();
            }
            finally
            {
                HandleFinally();
            }
        }

        private void HandleOperationCanceled()
        {
            if (Conversation.Count > 0 && Conversation[^1].Role == MessageRole.AI)
            {
                if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
                {
                    Conversation[^1].Text = "[Canceled response]";
                }
                else
                {
                    Conversation[^1].Text += " [canceled]";
                }
            }

        }

        private void HandleFinally()
        {
            IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
            NotifyChange();
        }

        private void DisplayNextApprovalRequest()
        {
            if (pendingApprovals.Count == 0)
            {
                PendingApproval = null;
                return;
            }

            var approvalRequest = pendingApprovals.Dequeue();

            if (approvalRequest.ToolCall is FunctionCallContent functionCall)
            {
                PendingApproval = new ApprovalRequestUI
                {
                    ApprovalRequest = approvalRequest,
                    ToolName = ConvertToolName(functionCall.Name),
                    Arguments = functionCall.Arguments?.ToDictionary(x => x.Key, x => x.Value) ?? []
                };
            }
        }

        public async Task SendMessageAsync(string userText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userText))
            {
                return;
            }

            if (IsProcessing || PendingApproval is not null)
            {
                return;
            }

            try
            {
                IsProcessing = true;
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                Conversation.Add(new ChatMessageUI { Role = DTOs.MessageRole.User, Text = userText });
                messages.Add(new ChatMessage(ChatRole.User, userText));

                Conversation.Add(new ChatMessageUI { Role = DTOs.MessageRole.AI, Text = string.Empty });
                NotifyChange();

                await SendMessagesToTheAssistant(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                HandleOperationCanceled();
            }
            finally
            {
                HandleFinally();
            }
        }

        private async Task SendMessagesToTheAssistant(CancellationToken cancellationToken = default)
        {
            var updates = new List<ChatResponseUpdate>();

            var chatClient = chatClientFactory.Create(this.model);

            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, chatOptions,
                                                                   cancellationToken: cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        Conversation[^1].Text += textContent.Text;
                        NotifyChange();
                    }
                }
            }

            var response = updates.ToChatResponse();
            messages.AddMessages(response);

            var approvalRequests = response.Messages
                       .SelectMany(m => m.Contents)
                       .OfType<ToolApprovalRequestContent>()
                       .ToList();

            if (approvalRequests.Count > 0)
            {
                foreach (var approvalRequest in approvalRequests)
                {
                    pendingApprovals.Enqueue(approvalRequest);
                }

                // Remove the empty bubble from the AI
                if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
                {
                    Conversation.RemoveAt(Conversation.Count - 1);
                }

                DisplayNextApprovalRequest();
                NotifyChange();
                return;
            }
        }

        private void NotifyChange() => OnChange?.Invoke();

        private static string ConvertToolName(string toolName)
        {
            return toolName switch
            {
                "SendEmail" => "Send email",
                _ => toolName
            };
        }

        public void SetModel(string model)
        {
            this.model = model;
        }
    }
}
