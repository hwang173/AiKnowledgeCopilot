using AiKnowledgeCopilot.API.Security;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.DocumentUser)]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISemanticSearchService
        _semanticSearchService;

    private readonly ICurrentUserContext
        _currentUserContext;

    public SearchController(
        ISemanticSearchService semanticSearchService,
        ICurrentUserContext currentUserContext)
    {
        _semanticSearchService =
            semanticSearchService;

        _currentUserContext =
            currentUserContext;
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

    private bool TryGetCurrentUserId(
        out string requestedByUserId)
    {
        requestedByUserId = string.Empty;

        if (!_currentUserContext.IsAuthenticated)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
            _currentUserContext.UserId))
        {
            return false;
        }

        requestedByUserId =
            _currentUserContext.UserId;

        return true;
    }

    private UnauthorizedObjectResult CreateUnauthorizedProblem()
    {
        var problemDetails =
            new ProblemDetails
            {
                Title = "Authentication is required.",
                Detail = "A valid bearer token with a user id claim is required.",
                Status = StatusCodes.Status401Unauthorized
            };

        return Unauthorized(problemDetails);
    }
}