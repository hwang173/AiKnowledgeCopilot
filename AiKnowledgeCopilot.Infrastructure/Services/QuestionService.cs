using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Application.Search;

namespace AiKnowledgeCopilot.Infrastructure.Services;

public class QuestionService
    : IQuestionService
{
    private readonly ISemanticSearchService
        _semanticSearchService;

    private readonly IGenerativeAiService
        _generativeAiService;

    public QuestionService(
        ISemanticSearchService semanticSearchService,
        IGenerativeAiService generativeAiService)
    {
        _semanticSearchService =
            semanticSearchService;

        _generativeAiService =
            generativeAiService;
    }

    public async Task<AnswerResponseDto> AskAsync(
        string question,
        CancellationToken cancellationToken)
    {
        var searchResults =
            await _semanticSearchService
                .SearchAsync(
                    question,
                    cancellationToken);

        var contextChunks =
            searchResults
                .Select(x => x.Content)
                .ToList();

        return await _generativeAiService
            .GenerateAnswerAsync(
                question,
                contextChunks,
                cancellationToken);
    }
}