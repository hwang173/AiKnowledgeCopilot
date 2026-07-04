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
    : AuthenticatedControllerBase
{
    private readonly IQuestionService
        _questionService;

    public QuestionController(
        IQuestionService questionService,
        ICurrentUserContext currentUserContext)
        : base(currentUserContext)
    {
        _questionService =
            questionService;
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
}