using Microsoft.Extensions.AI;
using WebAPI_AI.DTOs;

namespace WebAPI_AI.Services.Chatbots
{
    public class Chatbot
    {
        private readonly IChatClientFactory chatClientFactory;
        private readonly ChatOptions chatOptions;
        private readonly List<ChatMessage> messages = [];

        public List<ChatMessageUI> Conversation { get; } = [];
        public List<ToolApprovalRequestContent> GeneratedApprovalRequests { get; } = [];

        public Chatbot(IChatClientFactory chatClientFactory,
            ChatOptions chatOptions,
            IEnumerable<ChatMessageUI>? history = null)
        {
            this.chatClientFactory = chatClientFactory;
            this.chatOptions = chatOptions;
            var systemPromptGeneral = """
You are an assistant that answers general questions.
You must respond in English.
Responses must be plain text and should not use formatting such as Markdown.
Responses should be concise unless instructed otherwise.

If a tool fails, read the exception message to see if you can fix the issue by making adjustments. Inform the user of any adjustments you are going to make.
""";

            messages.Add(new ChatMessage(ChatRole.System, systemPromptGeneral));

            if (history is null)
                return;

            Conversation = history.ToList();

            foreach (var message in history)
            {
                if (message.Role == MessageRole.User)
                {
                    messages.Add(new ChatMessage(ChatRole.User, message.Text));
                }
                else if (message.Role == MessageRole.AI)
                {
                    messages.Add(new ChatMessage(ChatRole.Assistant, message.Text));
                }
            }

        }

        public async Task SendMessageStreamAsync(string userPrompt,
            Func<string, Task>? onChange,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                return;

            try
            {
                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.User,
                    Text = userPrompt
                });

                messages.Add(new ChatMessage(ChatRole.User, userPrompt));

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.AI,
                    Text = string.Empty
                });

                await SendMessagesToTheAssistant(cancellationToken, onChange);
            }
            catch (OperationCanceledException)
            {
                HandleOperationCanceled();
                throw;
            }
        }

        public async Task RespondToApprovalRequests(
    IEnumerable<ToolApprovalRequestContent> approvalRequests,
    IEnumerable<ToolApprovalResponseContent> approvalResponses,
    CancellationToken cancellationToken = default)
        {
            var requests = approvalRequests.Cast<AIContent>().ToList();
            var responses = approvalResponses.Cast<AIContent>().ToList();

            try
            {
                GeneratedApprovalRequests.Clear();

                messages.Add(new ChatMessage(ChatRole.Assistant, requests));
                messages.Add(new ChatMessage(ChatRole.User, responses));

                Conversation.Add(new ChatMessageUI
                {
                    Role = MessageRole.AI,
                    Text = string.Empty
                });

                await SendMessagesToTheAssistant(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                HandleOperationCanceled();
                throw;
            }
        }

        private async Task SendMessagesToTheAssistant(CancellationToken cancellationToken,
            Func<string, Task>? onDelta = null)
        {
            var updates = new List<ChatResponseUpdate>();
            var client = chatClientFactory.Create();

            await foreach(var update in client.GetStreamingResponseAsync(messages, chatOptions,
                cancellationToken: cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        Conversation[^1].Text += textContent.Text;
                        if (onDelta is not null)
                        {
                            await onDelta(textContent.Text);
                        }
                    }
                }
            }

            var response = updates.ToChatResponse();
            messages.AddMessages(response);

            var approvalRequests = response.Messages.SelectMany(m => m.Contents)
                                    .OfType<ToolApprovalRequestContent>().ToList();

            GeneratedApprovalRequests.AddRange(approvalRequests);

            if (string.IsNullOrWhiteSpace(Conversation[^1].Text))
            {
                Conversation.RemoveAt(Conversation.Count - 1);
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

        public static string ConvertFunctionName(string name)
        {
            return name switch
            {
                "SendEmail" => "Send email",
                _ => name
            };
        }

    }
}
