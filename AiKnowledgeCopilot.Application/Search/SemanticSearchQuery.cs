namespace AiKnowledgeCopilot.Application.Search;

public class SemanticSearchQuery
{
    public string Query { get; init; } = string.Empty;

    public string RequestedByUserId { get; init; } = string.Empty;
}