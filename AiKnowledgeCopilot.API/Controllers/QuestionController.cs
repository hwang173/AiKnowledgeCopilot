using AiKnowledgeCopilot.API.Security;
using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Security;
using AiKnowledgeCopilot.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.DocumentUser)]
[Route("api/questions")]
public class QuestionController
    : ControllerBase
{
    private readonly IQuestionService
        _questionService;

    private readonly ICurrentUserContext
        _currentUserContext;

    public QuestionController(
        IQuestionService questionService,
        ICurrentUserContext currentUserContext)
    {
        _questionService =
            questionService;

        _currentUserContext =
            currentUserContext;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AnswerResponseDto>>
        Ask(
            QuestionRequestDto request,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var requestedByUserId))
        {
            return CreateUnauthorizedProblem();
        }

        var answer =
            await _questionService
                .AskAsync(
                    new QuestionQuery
                    {
                        Question = request.Question,
                        RequestedByUserId =
                            requestedByUserId
                    },
                    cancellationToken);

        return Ok(answer);
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