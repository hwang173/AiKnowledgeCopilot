using AiKnowledgeCopilot.Domain.Enums;

namespace AiKnowledgeCopilot.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string UploadedByUserId { get; private set; } = string.Empty;

    public DateTime UploadedAtUtc { get; private set; }

    public DocumentStatus Status { get; private set; }

    public List<Chunk> Chunks { get; private set; } = new();

    private Document()
    {
    }

    public Document(
        string fileName,
        string uploadedByUserId)
    {
        Id = Guid.NewGuid();

        FileName = fileName;

        UploadedByUserId = uploadedByUserId;

        UploadedAtUtc = DateTime.UtcNow;

        Status = DocumentStatus.Uploaded;
    }

    public void MarkAsProcessing()
    {
        Status = DocumentStatus.Processing;
    }

    public void MarkAsCompleted()
    {
        Status = DocumentStatus.Completed;
    }

    public void MarkAsFailed()
    {
        Status = DocumentStatus.Failed;
    }

    public void AddChunk(Chunk chunk)
    {
        Chunks.Add(chunk);
    }
}