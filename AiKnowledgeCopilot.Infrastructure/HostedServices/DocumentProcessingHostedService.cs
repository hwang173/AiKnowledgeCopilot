using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Storage;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using AiKnowledgeCopilot.Application.Documents;
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

        var services =
            ResolveServices(scope.ServiceProvider);

        var document =
            await services.Repository.GetByIdAsync(
                documentId,
                cancellationToken);

        if (document is null)
        {
            return;
        }

        if (!await ValidateAndPrepareDocumentAsync(
            document,
            services,
            cancellationToken))
        {
            return;
        }

        await ProcessDocumentContentAsync(
            document,
            services,
            cancellationToken);

        await CompleteProcessingAsync(
            document,
            services,
            cancellationToken);
    }

    private ProcessingServices ResolveServices(
        IServiceProvider serviceProvider)
    {
        return new ProcessingServices(
            serviceProvider.GetRequiredService<IDocumentRepository>(),
            serviceProvider.GetRequiredService<AppDbContext>(),
            serviceProvider.GetRequiredService<IChunkingService>(),
            serviceProvider.GetRequiredService<IEmbeddingService>(),
            serviceProvider.GetRequiredService<ITextExtractionService>());
    }

    private async Task<bool> ValidateAndPrepareDocumentAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        document.MarkAsProcessing();

        if (!await ValidateFileExistsAsync(
            document,
            services,
            cancellationToken))
        {
            return false;
        }

        await services.Repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private async Task<bool> ValidateFileExistsAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(document.FilePath))
        {
            document.MarkAsFailed();

            await services.Repository.SaveChangesAsync(
                cancellationToken);

            _logger.LogError(
                "File not found: {FilePath}",
                document.FilePath);

            return false;
        }

        return true;
    }

    private async Task ProcessDocumentContentAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        var documentContent =
            await LoadDocumentContentAsync(
                document,
                services,
                cancellationToken);

        if (documentContent is null)
        {
            return;
        }

        var chunks =
            services.ChunkingService.Chunk(
                documentContent);

        await ProcessChunksAsync(
            document,
            chunks,
            services,
            cancellationToken);

        await Task.Delay(5000, cancellationToken);
    }

    private async Task<string?> LoadDocumentContentAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        var documentContent =
            await services.TextExtractionService
                .ExtractTextAsync(
                    document.FilePath,
                    cancellationToken);

        if (!string.IsNullOrWhiteSpace(documentContent))
        {
            return documentContent;
        }

        document.MarkAsFailed();

        await services.Repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogWarning(
            "Document {DocumentId} is empty.",
            document.Id);

        return null;
    }

    private async Task ProcessChunksAsync(
        Domain.Entities.Document document,
        IReadOnlyList<string> chunks,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            await ProcessChunkAsync(
                document,
                chunks[i],
                i,
                services,
                cancellationToken);
        }
    }

    private async Task ProcessChunkAsync(
        Domain.Entities.Document document,
        string chunkContent,
        int chunkIndex,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        var chunk = new Domain.Entities.Chunk(
            document.Id,
            chunkContent,
            chunkIndex);

        var embedding =
            await services.EmbeddingService
                .GenerateEmbeddingAsync(
                    chunkContent,
                    cancellationToken);

        chunk.SetEmbedding(embedding);

        document.AddChunk(chunk);

        services.DbContext.Entry(chunk).State =
            EntityState.Added;
    }

    private async Task CompleteProcessingAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        CancellationToken cancellationToken)
    {
        document.MarkAsCompleted();

        await services.Repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Document {DocumentId} processed.",
            document.Id);
    }

    private sealed record ProcessingServices(
        IDocumentRepository Repository,
        AppDbContext DbContext,
        IChunkingService ChunkingService,
        IEmbeddingService EmbeddingService,
        ITextExtractionService TextExtractionService);
}