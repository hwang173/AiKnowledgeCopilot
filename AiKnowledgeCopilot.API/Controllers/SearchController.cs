using AiKnowledgeCopilot.Application.RAG;
using AiKnowledgeCopilot.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISemanticSearchService
        _semanticSearchService;

    public SearchController(
        ISemanticSearchService semanticSearchService)
    {
        _semanticSearchService =
            semanticSearchService;
    }

    [HttpPost]
    public async Task<ActionResult<SearchResponse>>
        Search(
            SearchQuery query,
            CancellationToken cancellationToken)
    {

        var results =
            await _semanticSearchService
                .SearchAsync(
                    query.Query,
                    cancellationToken);

        var response =
            new SearchResponse
            {
                Results = results
            };

        return Ok(response);
    }
}