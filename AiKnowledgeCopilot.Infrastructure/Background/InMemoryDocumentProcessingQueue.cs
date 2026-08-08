using System.Threading.Channels;
using AiKnowledgeCopilot.Application.Background;

namespace AiKnowledgeCopilot.Infrastructure.Background;

public class InMemoryDocumentProcessingQueue
    : IDocumentProcessingQueue
{
    private readonly Channel<DocumentProcessingMessage>
        _queue;

    public InMemoryDocumentProcessingQueue()
    {
        _queue =
            Channel.CreateUnbounded<DocumentProcessingMessage>();
    }

    public async ValueTask QueueAsync(
        DocumentProcessingMessage message,
        CancellationToken cancellationToken)
    {
        ValidateMessage(message);

        await _queue.Writer.WriteAsync(
            message,
            cancellationToken);
    }

    public async ValueTask<DocumentProcessingMessage> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(
            cancellationToken);
    }

    private static void ValidateMessage(
        DocumentProcessingMessage message)
    {
        if (message.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "DocumentId is required.",
                nameof(message));
        }

        if (string.IsNullOrWhiteSpace(
            message.CorrelationId))
        {
            throw new ArgumentException(
                "CorrelationId is required.",
                nameof(message));
        }

        if (string.IsNullOrWhiteSpace(
            message.QueuedByUserId))
        {
            throw new ArgumentException(
                "QueuedByUserId is required.",
                nameof(message));
        }

        if (message.QueuedAtUtc == default)
        {
            throw new ArgumentException(
                "QueuedAtUtc is required.",
                nameof(message));
        }
    }
}