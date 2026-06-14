using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Domain.Entities;

namespace AiKnowledgeCopilot.Infrastructure.Services;

public class DocumentService
    : IDocumentService
{
    private readonly IDocumentRepository
        _documentRepository;

    private readonly IDocumentProcessingQueue
        _queue;

    public DocumentService(
        IDocumentRepository documentRepository,
        IDocumentProcessingQueue queue)
    {
        _documentRepository =
            documentRepository;

        _queue =
            queue;
    }

    public async Task<Guid> UploadAsync(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        var document =
            new Document(
                command.FileName,
                command.FilePath,
                command.UploadedByUserId);

        await _documentRepository.AddAsync(
            document,
            cancellationToken);

        await _documentRepository.SaveChangesAsync(
            cancellationToken);

        await _queue.QueueAsync(
            document.Id);

        return document.Id;
    }

    private static void ValidateCommand(
        UploadDocumentCommand command)
    {
        if (string.IsNullOrWhiteSpace(
            command.FileName))
        {
            throw new ArgumentException(
                "FileName is required.");
        }

        if (string.IsNullOrWhiteSpace(
            command.FilePath))
        {
            throw new ArgumentException(
                "FilePath is required.");
        }

        if (string.IsNullOrWhiteSpace(
            command.UploadedByUserId))
        {
            throw new ArgumentException(
                "UploadedByUserId is required.");
        }
    }
}