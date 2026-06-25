namespace AiKnowledgeCopilot.Application.Documents;

public class DocumentStatusDto
{
    public Guid DocumentId { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string UploadedByUserId { get; init; } = string.Empty;

    public DateTime UploadedAtUtc { get; init; }

    public DateTime? ProcessingStartedAtUtc { get; init; }

    public DateTime? ProcessingCompletedAtUtc { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? FailureReason { get; init; }

    public int ChunkCount { get; init; }
}