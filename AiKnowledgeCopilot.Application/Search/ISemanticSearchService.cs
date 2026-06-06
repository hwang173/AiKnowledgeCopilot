namespace AiKnowledgeCopilot.Application.Search;

public interface ISemanticSearchService
{
    Task<List<SearchResultDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken);
}