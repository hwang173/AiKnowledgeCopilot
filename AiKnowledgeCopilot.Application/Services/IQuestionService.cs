using AiKnowledgeCopilot.Application.AI;

namespace AiKnowledgeCopilot.Application.Services;

public interface IQuestionService
{
    Task<AnswerResponseDto> AskAsync(
        string question,
        CancellationToken cancellationToken);
}