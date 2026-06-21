using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.HostedServices;

public class DocumentProcessingHostedService
    : BackgroundService
{
    private const int MaxFailureReasonLength = 2000;

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
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Document processing worker is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in document processing worker loop.");
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
            _logger.LogWarning(
                "Document {DocumentId} was not found.",
                documentId);

            return;
        }

        try
        {
            if (!await ValidateAndPrepareDocumentAsync(
                document,
                services,
                cancellationToken))
            {
                return;
            }

            if (!await ProcessDocumentContentAsync(
                document,
                services,
                cancellationToken))
            {
                return;
            }

            await CompleteProcessingAsync(
                document,
                services,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await FailDocumentAsync(
                document,
                services,
                ex,
                cancellationToken);
        }
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
        if (File.Exists(document.FilePath))
        {
            return true;
        }

        var failureReason =
            $"File not found: {document.FilePath}";

        document.MarkAsFailed(failureReason);

        await services.Repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogError(
            "Document {DocumentId} failed because file was not found: {FilePath}",
            document.Id,
            document.FilePath);

        return false;
    }

    private async Task<bool> ProcessDocumentContentAsync(
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
            return false;
        }

        var chunks =
            services.ChunkingService.Chunk(
                documentContent);

        if (chunks.Count == 0)
        {
            await FailDocumentAsync(
                document,
                services,
                "Document did not produce any searchable chunks.",
                cancellationToken);

            return false;
        }

        await ProcessChunksAsync(
            document,
            chunks,
            services,
            cancellationToken);

        return true;
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

        await FailDocumentAsync(
            document,
            services,
            "Document content is empty after text extraction.",
            cancellationToken);

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
            "Document {DocumentId} processed successfully.",
            document.Id);
    }

    private async Task FailDocumentAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var failureReason =
            CreateFailureReason(exception);

        await FailDocumentAsync(
            document,
            services,
            failureReason,
            cancellationToken);

        _logger.LogError(
            exception,
            "Document {DocumentId} failed. Reason: {FailureReason}",
            document.Id,
            failureReason);
    }

    private async Task FailDocumentAsync(
        Domain.Entities.Document document,
        ProcessingServices services,
        string failureReason,
        CancellationToken cancellationToken)
    {
        DiscardPendingChunks(
            document,
            services.DbContext);

        document.MarkAsFailed(failureReason);

        await services.Repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogWarning(
            "Document {DocumentId} marked as failed. Reason: {FailureReason}",
            document.Id,
            failureReason);
    }

    private static void DiscardPendingChunks(
        Domain.Entities.Document document,
        AppDbContext dbContext)
    {
        var pendingChunkEntries =
            dbContext.ChangeTracker
                .Entries<Domain.Entities.Chunk>()
                .Where(x => x.State == EntityState.Added)
                .ToList();

        foreach (var entry in pendingChunkEntries)
        {
            document.Chunks.Remove(entry.Entity);

            entry.State = EntityState.Detached;
        }
    }

    private static string CreateFailureReason(
        Exception exception)
    {
        var failureReason =
            string.IsNullOrWhiteSpace(exception.Message)
                ? exception.GetType().Name
                : $"{exception.GetType().Name}: {exception.Message}";

        if (failureReason.Length <= MaxFailureReasonLength)
        {
            return failureReason;
        }

        return failureReason[..MaxFailureReasonLength];
    }

    private sealed record ProcessingServices(
        IDocumentRepository Repository,
        AppDbContext DbContext,
        IChunkingService ChunkingService,
        IEmbeddingService EmbeddingService,
        ITextExtractionService TextExtractionService);
}