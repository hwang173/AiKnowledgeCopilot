using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Application.Services;

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
        QuestionQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var searchResults =
            await _semanticSearchService
                .SearchAsync(
                    new SemanticSearchQuery
                    {
                        Query = query.Question,
                        RequestedByUserId =
                            query.RequestedByUserId
                    },
                    cancellationToken);

        var contextChunks =
            searchResults
                .Select(x => x.Content)
                .ToList();

        return await _generativeAiService
            .GenerateAnswerAsync(
                query.Question,
                contextChunks,
                cancellationToken);
    }

    private static void ValidateQuery(
        QuestionQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Question))
        {
            throw new ArgumentException(
                "Question is required.",
                nameof(query));
        }

        if (string.IsNullOrWhiteSpace(
            query.RequestedByUserId))
        {
            throw new ArgumentException(
                "RequestedByUserId is required.",
                nameof(query));
        }
    }
}