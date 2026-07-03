namespace AiKnowledgeCopilot.Application.Search;

public interface ISemanticSearchService
{
    Task<List<SearchResultDto>> SearchAsync(
        SemanticSearchQuery query,
        CancellationToken cancellationToken);
}