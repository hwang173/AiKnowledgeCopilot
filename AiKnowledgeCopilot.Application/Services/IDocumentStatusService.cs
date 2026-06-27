using AiKnowledgeCopilot.Application.Documents;

namespace AiKnowledgeCopilot.Application.Services;

public interface IDocumentStatusService
{
    Task<DocumentStatusDto?> GetByIdAsync(
        DocumentStatusQuery query,
        CancellationToken cancellationToken);
}