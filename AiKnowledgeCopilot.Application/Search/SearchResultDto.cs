namespace AiKnowledgeCopilot.Application.Search;

public class SearchResultDto
{
    public Guid ChunkId { get; set; }

    public Guid DocumentId { get; set; }

    public string DocumentFileName { get; set; }
        = string.Empty;

    public string Content { get; set; }
        = string.Empty;

    public double Similarity { get; set; }
}