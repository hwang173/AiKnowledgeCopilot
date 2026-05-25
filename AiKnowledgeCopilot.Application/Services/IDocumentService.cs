using AiKnowledgeCopilot.Domain.Entities;

namespace AiKnowledgeCopilot.Application.Services;

public interface IDocumentService
{
    Task<Guid> UploadAsync(
        Documents.UploadDocumentRequest request,
        CancellationToken cancellationToken);
}