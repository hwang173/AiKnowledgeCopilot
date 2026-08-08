using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Observability;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.Services;

public class DocumentService
    : IDocumentService
{
    private readonly IDocumentRepository
        _documentRepository;

    private readonly IDocumentProcessingQueue
        _queue;

    private readonly ICorrelationContext
        _correlationContext;

    private readonly ILogger<DocumentService>
        _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IDocumentProcessingQueue queue,
        ICorrelationContext correlationContext,
        ILogger<DocumentService> logger)
    {
        _documentRepository =
            documentRepository;

        _queue =
            queue;

        _correlationContext =
            correlationContext;

        _logger =
            logger;
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

        var message =
            CreateProcessingMessage(
                document,
                command.UploadedByUserId);

        await _queue.QueueAsync(
            message,
            cancellationToken);

        _logger.LogInformation(
            "Document {DocumentId} queued for processing by user {QueuedByUserId}.",
            message.DocumentId,
            message.QueuedByUserId);

        return document.Id;
    }

    private DocumentProcessingMessage CreateProcessingMessage(
        Document document,
        string queuedByUserId)
    {
        return new DocumentProcessingMessage
        {
            DocumentId = document.Id,

            CorrelationId =
                _correlationContext.CorrelationId,

            QueuedByUserId =
                queuedByUserId,

            QueuedAtUtc =
                DateTime.UtcNow
        };
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