namespace AiKnowledgeCopilot.Application.Background;

public class DocumentProcessingQueueFullException : Exception
{
    public DocumentProcessingQueueFullException(
        int capacity,
        TimeSpan enqueueTimeout)
        : base(
            $"Document processing queue is full. Capacity: {capacity}. Enqueue timeout: {enqueueTimeout.TotalSeconds} seconds.")
    {
        Capacity = capacity;

        EnqueueTimeout = enqueueTimeout;
    }

    public int Capacity { get; }

    public TimeSpan EnqueueTimeout { get; }
}