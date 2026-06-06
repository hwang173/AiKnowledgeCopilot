namespace AiKnowledgeCopilot.Application.Search;

public class SearchResultDto
{
    public Guid ChunkId { get; set; }

    public string Content { get; set; }
        = string.Empty;

    public double Similarity { get; set; }
}