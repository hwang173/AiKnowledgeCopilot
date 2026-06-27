namespace AiKnowledgeCopilot.Application.Documents;

public class DocumentStatusQuery
{
    public Guid DocumentId { get; init; }

    public string RequestedByUserId { get; init; } = string.Empty;
}