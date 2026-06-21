using AiKnowledgeCopilot.Domain.Enums;

namespace AiKnowledgeCopilot.Domain.Entities;

public class Document
{
    private const int MaxFailureReasonLength = 2000;

    public Guid Id { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string FilePath { get; private set; }
        = string.Empty;

    public string UploadedByUserId { get; private set; } = string.Empty;

    public DateTime UploadedAtUtc { get; private set; }

    public DateTime? ProcessingStartedAtUtc { get; private set; }

    public DateTime? ProcessingCompletedAtUtc { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public List<Chunk> Chunks { get; private set; } = new();

    private Document()
    {
    }

    public Document(
        string fileName,
        string filePath,
        string uploadedByUserId)
    {
        Id = Guid.NewGuid();

        FileName = fileName;

        FilePath = filePath;

        UploadedByUserId = uploadedByUserId;

        UploadedAtUtc = DateTime.UtcNow;

        Status = DocumentStatus.Uploaded;
    }

    public void MarkAsProcessing()
    {
        Status = DocumentStatus.Processing;

        ProcessingStartedAtUtc = DateTime.UtcNow;

        ProcessingCompletedAtUtc = null;

        FailureReason = null;
    }

    public void MarkAsCompleted()
    {
        Status = DocumentStatus.Completed;

        ProcessingCompletedAtUtc = DateTime.UtcNow;

        FailureReason = null;
    }

    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException(
                "Failure reason is required.",
                nameof(failureReason));
        }

        Status = DocumentStatus.Failed;

        ProcessingCompletedAtUtc = DateTime.UtcNow;

        FailureReason =
            NormalizeFailureReason(failureReason);
    }

    public void AddChunk(Chunk chunk)
    {
        Chunks.Add(chunk);
    }

    private static string NormalizeFailureReason(
        string failureReason)
    {
        var normalizedReason =
            failureReason.Trim();

        if (normalizedReason.Length <= MaxFailureReasonLength)
        {
            return normalizedReason;
        }

        return normalizedReason[..MaxFailureReasonLength];
    }
}