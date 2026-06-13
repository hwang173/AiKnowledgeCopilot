namespace AiKnowledgeCopilot.Application.AI;

public interface IGenerativeAiService
{
    Task<AnswerResponseDto> GenerateAnswerAsync(
        string question,
        List<string> contextChunks,
        CancellationToken cancellationToken);
}