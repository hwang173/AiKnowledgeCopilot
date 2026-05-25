using System.Threading.Channels;
using AiKnowledgeCopilot.Application.Background;

namespace AiKnowledgeCopilot.Infrastructure.Background;

public class InMemoryDocumentProcessingQueue
    : IDocumentProcessingQueue
{
    private readonly Channel<Guid> _queue;

    public InMemoryDocumentProcessingQueue()
    {
        _queue = Channel.CreateUnbounded<Guid>();
    }

    public async ValueTask QueueAsync(Guid documentId)
    {
        await _queue.Writer.WriteAsync(documentId);
    }

    public async ValueTask<Guid> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(
            cancellationToken);
    }
}