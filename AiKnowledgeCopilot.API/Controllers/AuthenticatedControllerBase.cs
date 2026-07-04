using AiKnowledgeCopilot.Application.Security;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

public abstract class AuthenticatedControllerBase : ControllerBase
{
    private readonly ICurrentUserContext
        _currentUserContext;

    protected AuthenticatedControllerBase(
        ICurrentUserContext currentUserContext)
    {
        _currentUserContext =
            currentUserContext;
    }

    protected bool TryGetCurrentUserId(
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

    protected UnauthorizedObjectResult CreateUnauthorizedProblem()
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