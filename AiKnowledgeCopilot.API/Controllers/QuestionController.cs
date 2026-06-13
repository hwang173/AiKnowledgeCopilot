using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionController
    : ControllerBase
{
    private readonly IQuestionService
        _questionService;

    public QuestionController(
        IQuestionService questionService)
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
        var answer =
            await _questionService
                .AskAsync(
                    request.Question,
                    cancellationToken);

        return Ok(answer);
    }
}