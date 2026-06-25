using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Domain.Entities;

namespace AiKnowledgeCopilot.Infrastructure.Services;

public class DocumentStatusService
    : IDocumentStatusService
{
    private readonly IDocumentRepository
        _documentRepository;

    public DocumentStatusService(
        IDocumentRepository documentRepository)
    {
        _documentRepository =
            documentRepository;
    }

    public async Task<DocumentStatusDto?> GetByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (documentId == Guid.Empty)
        {
            throw new ArgumentException(
                "DocumentId is required.",
                nameof(documentId));
        }

        var document =
            await _documentRepository.GetByIdAsync(
                documentId,
                cancellationToken);

        if (document is null)
        {
            return null;
        }

        return MapToDto(document);
    }

    private static DocumentStatusDto MapToDto(
        Document document)
    {
        return new DocumentStatusDto
        {
            DocumentId = document.Id,

            FileName = document.FileName,

            UploadedByUserId = document.UploadedByUserId,

            UploadedAtUtc = document.UploadedAtUtc,

            ProcessingStartedAtUtc =
                document.ProcessingStartedAtUtc,

            ProcessingCompletedAtUtc =
                document.ProcessingCompletedAtUtc,

            Status = document.Status.ToString(),

            FailureReason = document.FailureReason,

            ChunkCount = document.Chunks.Count
        };
    }
}