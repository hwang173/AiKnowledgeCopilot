namespace AiKnowledgeCopilot.Application.Background;

public class DocumentProcessingMessage
{
    public Guid DocumentId { get; init; }

    public string CorrelationId { get; init; } = string.Empty;

    public string QueuedByUserId { get; init; } = string.Empty;

    public DateTime QueuedAtUtc { get; init; }

    public string? TraceParent { get; init; }

    public string? TraceState { get; init; }
}