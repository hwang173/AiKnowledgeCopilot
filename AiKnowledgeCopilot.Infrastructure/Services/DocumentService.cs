using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Domain.Entities;
using AiKnowledgeCopilot.Application.Background;

namespace AiKnowledgeCopilot.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentProcessingQueue _queue;

    public DocumentService(
        IDocumentRepository documentRepository,
        IDocumentProcessingQueue queue)
    {
        _documentRepository = documentRepository;
        _queue = queue;
    }

    public async Task<Guid> UploadAsync(
        UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var document = new Document(
            request.FileName,
            request.UploadedByUserId);

        await _documentRepository.AddAsync(
            document,
            cancellationToken);

        await _documentRepository.SaveChangesAsync(
            cancellationToken);

        await _queue.QueueAsync(document.Id);

        return document.Id;
    }

    private static void ValidateRequest(
        UploadDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException(
                "File name is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.UploadedByUserId))
        {
            throw new ArgumentException(
                "UploadedByUserId is required.");
        }
    }
}