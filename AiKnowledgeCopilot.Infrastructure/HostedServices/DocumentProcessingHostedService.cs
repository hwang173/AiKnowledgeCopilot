using System.Diagnostics;
using AiKnowledgeCopilot.Application.Observability;
using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Infrastructure.Background;
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
    private readonly DocumentProcessingQueueOptions _options;
    private readonly ILogger<DocumentProcessingHostedService>
        _logger;

    public DocumentProcessingHostedService(
        IDocumentProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        DocumentProcessingQueueOptions options,
        ILogger<DocumentProcessingHostedService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Document processing worker started with {ProcessorCount} processors.",
            _options.MaxConcurrentProcessors);

        var processorTasks =
            Enumerable
                .Range(1, _options.MaxConcurrentProcessors)
                .Select(processorId =>
                    RunProcessorAsync(
                        processorId,
                        stoppingToken))
                .ToList();

        await Task.WhenAll(processorTasks);
    }

    private async Task RunProcessorAsync(
        int processorId,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Document processor {ProcessorId} started.",
            processorId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message =
                    await _queue.DequeueAsync(
                        stoppingToken);

                using var logScope =
                    BeginProcessingLogScope(
                        processorId,
                        message);

                using var activity =
                    StartProcessingActivity(
                        processorId,
                        message);

                var queueDelayMs =
                    (DateTime.UtcNow - message.QueuedAtUtc)
                        .TotalMilliseconds;

                activity?.SetTag(
                    "messaging.queue.delay_ms",
                    queueDelayMs);

                AiKnowledgeCopilotTelemetry.DocumentQueueDelayHistogram.Record(
                    queueDelayMs);

                _logger.LogInformation(
                    "Processor {ProcessorId} dequeued document processing message after {QueueDelayMs} ms.",
                    processorId,
                    queueDelayMs);

                var processingStopwatch =
                    Stopwatch.StartNew();

                try
                {
                    await ProcessDocumentAsync(
                        message,
                        stoppingToken);
                }
                finally
                {
                    AiKnowledgeCopilotTelemetry.DocumentProcessingDurationHistogram.Record(
                        processingStopwatch.Elapsed.TotalMilliseconds);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Document processor {ProcessorId} is stopping.",
                    processorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in document processor {ProcessorId}.",
                    processorId);
            }
        }
    }

    private IDisposable? BeginProcessingLogScope(
        int processorId,
        DocumentProcessingMessage message)
    {
        return _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["ProcessorId"] = processorId,
                ["CorrelationId"] = message.CorrelationId,
                ["DocumentId"] = message.DocumentId,
                ["QueuedByUserId"] = message.QueuedByUserId,
                ["QueuedAtUtc"] = message.QueuedAtUtc
            });
    }

    private async Task ProcessDocumentAsync(
        DocumentProcessingMessage message,
        CancellationToken cancellationToken)
    {
        using var serviceScope =
            _scopeFactory.CreateScope();

        var services =
            ResolveServices(serviceScope.ServiceProvider);

        var document =
            await services.Repository.GetByIdAsync(
                message.DocumentId,
                cancellationToken);

        if (document is null)
        {
            _logger.LogWarning(
                "Document {DocumentId} was not found.",
                message.DocumentId);

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

        _logger.LogInformation(
            "Document {DocumentId} marked as processing.",
            document.Id);

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

        Activity.Current?.SetTag(
            "document.chunk_count",
            chunks.Count);

        _logger.LogInformation(
            "Document {DocumentId} produced {ChunkCount} chunks.",
            document.Id,
            chunks.Count);

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
            _logger.LogInformation(
                "Extracted text from document {DocumentId}. TextLength={TextLength}.",
                document.Id,
                documentContent.Length);

            Activity.Current?.SetTag(
                "document.text_length",
                documentContent.Length);

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
        var chunk =
            new Domain.Entities.Chunk(
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

        _logger.LogInformation(
            "Processed chunk {ChunkIndex} for document {DocumentId}.",
            chunkIndex,
            document.Id);
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

        AiKnowledgeCopilotTelemetry.DocumentProcessingSucceededCounter.Add(
            1);

        Activity.Current?.SetStatus(
            ActivityStatusCode.Ok);
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

        Activity.Current?.SetStatus(
            ActivityStatusCode.Error,
            failureReason);

        Activity.Current?.AddEvent(
            new ActivityEvent(
                "document.processing.failed",
                tags: new ActivityTagsCollection
                {
                    ["exception.type"] = exception.GetType().FullName,
                    ["exception.message"] = exception.Message
                }));
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

        AiKnowledgeCopilotTelemetry.DocumentProcessingFailedCounter.Add(
            1);

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

    private static Activity? StartProcessingActivity(
    int processorId,
    DocumentProcessingMessage message)
    {
        ActivityContext parentContext =
            default;

        var hasParentContext =
            !string.IsNullOrWhiteSpace(message.TraceParent) &&
            ActivityContext.TryParse(
                message.TraceParent,
                message.TraceState,
                out parentContext);

        var activity =
            hasParentContext
                ? AiKnowledgeCopilotTelemetry.ActivitySource.StartActivity(
                    "document.process",
                    ActivityKind.Consumer,
                    parentContext)
                : AiKnowledgeCopilotTelemetry.ActivitySource.StartActivity(
                    "document.process",
                    ActivityKind.Consumer);

        activity?.SetTag(
            "processor.id",
            processorId);

        activity?.SetTag(
            "document.id",
            message.DocumentId);

        activity?.SetTag(
            "user.id",
            message.QueuedByUserId);

        activity?.SetTag(
            "messaging.system",
            "in-memory-channel");

        activity?.SetTag(
            "messaging.operation",
            "process");

        activity?.SetTag(
            "messaging.destination.name",
            "document-processing");

        return activity;
    }

    private sealed record ProcessingServices(
        IDocumentRepository Repository,
        AppDbContext DbContext,
        IChunkingService ChunkingService,
        IEmbeddingService EmbeddingService,
        ITextExtractionService TextExtractionService);
}