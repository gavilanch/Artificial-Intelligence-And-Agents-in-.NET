using Anthropic.Models.Messages;
using BlazorAI.DTOs;
using BlazorAI.Services.Chatbots;
using BlazorAI.Utilities;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;
using System.Text;
using System.Text.Json;

namespace BlazorAI.Services.RAG
{
    public class ChatbotRAG : IChatbot
    {
        private readonly List<ChatMessage> messages = [];
        private readonly IChatClientFactory chatClientFactory;
        private readonly ChatOptions chatOptions;
        private readonly IRAGService ragService;
        private CancellationTokenSource? _cts;
        private string model;

        private readonly Queue<ToolApprovalRequestContent> pendingApprovals = new();

        public List<ChatMessageUI> Conversation { get; } = [];

        public ChatbotRAG(IChatClientFactory chatClientFactory, ChatOptions chatOptions, IRAGService ragService)
        {
            model = AIModels.GetDefaultModel;
            this.chatClientFactory = chatClientFactory;
            this.chatOptions = chatOptions;
            this.ragService = ragService;
            var systemPrompt = """
                    You are an assistant specialized exclusively in answering questions using the context retrieved from internal documents.

                    You must respond in English.
                    Responses must be in plain text, without markdown.

                    Mandatory rules:

                    - Answer only with information contained in the retrieved context.
                    - If the answer is not explicitly in the context, you must respond: "I do not have sufficient information in the documents to answer that question."
                    - Do not use the model’s general knowledge.
                    - Do not invent information.
                    - Do not answer questions about programming, general knowledge, mathematics, or other topics if they do not appear in the retrieved context.
                    - If the question is not related to the documents, reject it briefly.
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

        public Task ResolveApprovalAsync(bool approved, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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

                await SendMessagesToTheAssistant(userText, _cts.Token);
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

        private async Task SendMessagesToTheAssistant(string usersPrompt, CancellationToken cancellationToken = default)
        {
            var context = await ragService.FindRelevantContext(usersPrompt, top: 3, minScore: 0.6f, cancellationToken);

            if (context.Count == 0)
            {
                Conversation[^1].Text = "I do not have sufficient information in the documents to answer that question.";
                NotifyChange();
                return; 
            }

            var sourcesDelimiter = "|";

            /*
              Document: document-1
              Content: content of the document

              ---

              Document: document-2
              Content: content of the document 2
             */
            var messageContext = new ChatMessage(
                 ChatRole.System,
                 $$"""
                 Context recovered from the documents:

                 {{string.Join("\n\n---\n\n", context)}}

                 Question from the user:
                 {{usersPrompt}}

                 Instructions:
                 - Answer only if the response is explicitly supported by the retrieved context.
                 - If it is not, respond exactly:
                    "I do not have sufficient information in the documents to answer that question."
                 - First, write only the response for the user in plain text.
                 - Then, on a new line, write exactly:
                 {{sourcesDelimiter}}
                 - After the delimiter, write valid JSON using this format:
                 {"usedSources":["Document-1", "Document-2"]}
                 - For example: The document name may look like this: "code-of-conduct.md", where code-of-conduct.md would be the title you should place in usedSources.
                 - In "usedSources", include only the document titles from the sources that were actually used.
                 - Do not include irrelevant sources.
                 """);

            var messagesToSend = new List<ChatMessage>();
            messagesToSend.AddRange(messages);
            messagesToSend.Insert(messages.Count - 1, messageContext);

            var updates = new List<ChatResponseUpdate>();

            var chatClient = chatClientFactory.Create(this.model);

            var sb = new StringBuilder();
            var delimiterFound = false;

            await foreach (var update in chatClient.GetStreamingResponseAsync(messagesToSend, chatOptions,
                                                                   cancellationToken: cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {

                        if (textContent.Text.Contains(sourcesDelimiter) || delimiterFound)
                        {
                            sb.Append(textContent.Text);
                            delimiterFound = true;
                            continue;
                        } else
                        {
                            Conversation[^1].Text += textContent.Text;
                            NotifyChange();
                        }
                    }
                }
            }

            var sourcesContent = sb.ToString().Trim().Replace(sourcesDelimiter, "")
                                    .Replace("\r\n", "")
                                    .Replace("\n", "")
                                    .Replace("\r", "");
            var metadata = JsonSerializer.Deserialize<MetadataSources>(sourcesContent)!;

            Conversation[^1].CitedFiles = metadata.UsedSources.Select(usedSource => 
                                        new CitedFile { FileName = usedSource }).ToList();
            var response = updates.ToChatResponse();
            messages.AddMessages(response);
        }

        private void NotifyChange() => OnChange?.Invoke();

        public void SetModel(string model)
        {
            this.model = model;
        }
    }
}
