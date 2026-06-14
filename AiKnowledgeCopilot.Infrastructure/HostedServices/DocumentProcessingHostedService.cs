using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Storage;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.HostedServices;

public class DocumentProcessingHostedService
    : BackgroundService
{
    private readonly IDocumentProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessingHostedService>
        _logger;

    public DocumentProcessingHostedService(
        IDocumentProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentProcessingHostedService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Document processing worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var documentId =
                    await _queue.DequeueAsync(stoppingToken);

                await ProcessDocumentAsync(
                    documentId,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing document.");
            }
        }
    }

    private async Task ProcessDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<IDocumentRepository>();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var chunkingService =
            scope.ServiceProvider
                .GetRequiredService<IChunkingService>();

        var embeddingService =
            scope.ServiceProvider
                .GetRequiredService<IEmbeddingService>();

        var fileStorageService =
            scope.ServiceProvider
                .GetRequiredService<IFileStorageService>();

        var document =
            await repository.GetByIdAsync(
                documentId,
                cancellationToken);

        if (document is null)
        {
            return;
        }

        document.MarkAsProcessing();

        if (!File.Exists(document.FilePath))
        {
            document.MarkAsFailed();

            await repository.SaveChangesAsync(
                cancellationToken);

            _logger.LogError(
                "File not found: {FilePath}",
                document.FilePath);

            return;
        }

        await repository.SaveChangesAsync(
            cancellationToken);

        var documentContent =
            await fileStorageService
                .ReadTextAsync(
                    document.FilePath,
                    cancellationToken);

        if (string.IsNullOrWhiteSpace(
        documentContent))
        {
            document.MarkAsFailed();

            await repository.SaveChangesAsync(
                cancellationToken);

            _logger.LogWarning(
                "Document {DocumentId} is empty.",
                document.Id);

            return;
        }

        var chunks =
            chunkingService.Chunk(
                documentContent);

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = new Domain.Entities.Chunk(
                document.Id,
                chunks[i],
                i);

            var embedding =
                await embeddingService
                    .GenerateEmbeddingAsync(
                        chunks[i],
                        cancellationToken);

            chunk.SetEmbedding(embedding);

            document.AddChunk(chunk);

            dbContext.Entry(chunk).State =
                EntityState.Added;
        }

        await Task.Delay(5000, cancellationToken);

        document.MarkAsCompleted();

        await repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Document {DocumentId} processed.",
            documentId);
    }
}