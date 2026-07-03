namespace AiKnowledgeCopilot.Application.AI;

public class QuestionQuery
{
    public string Question { get; init; } = string.Empty;

    public string RequestedByUserId { get; init; } = string.Empty;
}