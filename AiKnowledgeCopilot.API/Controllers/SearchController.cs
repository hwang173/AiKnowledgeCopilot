using AiKnowledgeCopilot.API.Security;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.DocumentUser)]
[Route("api/[controller]")]
public class SearchController : AuthenticatedControllerBase
{
    private readonly ISemanticSearchService
        _semanticSearchService;

    public SearchController(
        ISemanticSearchService semanticSearchService,
        ICurrentUserContext currentUserContext)
        : base(currentUserContext)
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
        if (!TryGetCurrentUserId(out var requestedByUserId))
        {
            return CreateUnauthorizedProblem();
        }

        var results =
            await _semanticSearchService
                .SearchAsync(
                    new SemanticSearchQuery
                    {
                        Query = query.Query,
                        RequestedByUserId =
                            requestedByUserId
                    },
                    cancellationToken);

        var response =
            new SearchResponse
            {
                Results = results
            };

        return Ok(response);
    }
}