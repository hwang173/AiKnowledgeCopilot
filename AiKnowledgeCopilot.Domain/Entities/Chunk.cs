namespace AiKnowledgeCopilot.Domain.Entities;

public class Chunk
{
    public Guid Id { get; private set; }

    public Guid DocumentId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public int ChunkIndex { get; private set; }

    private Chunk()
    {
    }

    public Chunk(
        Guid documentId,
        string content,
        int chunkIndex)
    {
        Id = Guid.NewGuid();

        DocumentId = documentId;

        Content = content;

        ChunkIndex = chunkIndex;
    }
}