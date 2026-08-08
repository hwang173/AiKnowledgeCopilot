namespace AiKnowledgeCopilot.Application.Background;

public interface IDocumentProcessingQueue
{
    ValueTask QueueAsync(
        DocumentProcessingMessage message,
        CancellationToken cancellationToken);

    ValueTask<DocumentProcessingMessage> DequeueAsync(
        CancellationToken cancellationToken);
}