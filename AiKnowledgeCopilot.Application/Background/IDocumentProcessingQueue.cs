namespace AiKnowledgeCopilot.Application.Background;

public interface IDocumentProcessingQueue
{
    ValueTask QueueAsync(Guid documentId);

    ValueTask<Guid> DequeueAsync(
        CancellationToken cancellationToken);
}