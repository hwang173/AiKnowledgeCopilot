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
        DocumentStatusQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var document =
            await _documentRepository.GetByIdForUserAsync(
                query.DocumentId,
                query.RequestedByUserId,
                cancellationToken);

        if (document is null)
        {
            return null;
        }

        return MapToDto(document);
    }

    private static void ValidateQuery(
        DocumentStatusQuery query)
    {
        if (query.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "DocumentId is required.",
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