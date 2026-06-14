using AiKnowledgeCopilot.Application.Documents;

namespace AiKnowledgeCopilot.Application.Services;

public interface IDocumentService
{
    Task<Guid> UploadAsync(
        UploadDocumentCommand command,
        CancellationToken cancellationToken);
}