using System.Diagnostics.Metrics;
using System.Threading.Channels;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Observability;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.Background;

public class InMemoryDocumentProcessingQueue
    : IDocumentProcessingQueue
{
    private readonly Channel<DocumentProcessingMessage>
        _queue;

    private readonly DocumentProcessingQueueOptions
        _options;

    private readonly ILogger<InMemoryDocumentProcessingQueue>
        _logger;

    private readonly ObservableGauge<int>
        _queueDepthGauge;

    private int _queueDepth;

    public InMemoryDocumentProcessingQueue(
        DocumentProcessingQueueOptions options,
        ILogger<InMemoryDocumentProcessingQueue> logger)
    {
        _options =
            options;

        _logger =
            logger;

        ValidateOptions(_options);

        _queue =
            Channel.CreateBounded<DocumentProcessingMessage>(
                new BoundedChannelOptions(
                    _options.Capacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = false,
                    SingleWriter = false
                });

        _queueDepthGauge =
            AiKnowledgeCopilotTelemetry.Meter.CreateObservableGauge(
                name: "aiknowledgecopilot.document_processing.queue_depth",
                observeValue: () => Volatile.Read(ref _queueDepth),
                unit: "messages",
                description: "Current number of document processing messages waiting in the in-memory queue.");
    }

    public async ValueTask QueueAsync(
        DocumentProcessingMessage message,
        CancellationToken cancellationToken)
    {
        ValidateMessage(message);

        using var timeoutTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutTokenSource.CancelAfter(
            TimeSpan.FromSeconds(
                _options.EnqueueTimeoutSeconds));

        try
        {
            await _queue.Writer.WriteAsync(
                message,
                timeoutTokenSource.Token);

            Interlocked.Increment(
                ref _queueDepth);

            AiKnowledgeCopilotTelemetry.DocumentsQueuedCounter.Add(
                1);

            _logger.LogInformation(
                "Document processing message {DocumentId} enqueued. QueueCapacity={QueueCapacity}.",
                message.DocumentId,
                _options.Capacity);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Document processing queue is full. Document {DocumentId} could not be enqueued within {EnqueueTimeoutSeconds} seconds. QueueCapacity={QueueCapacity}.",
                message.DocumentId,
                _options.EnqueueTimeoutSeconds,
                _options.Capacity);

            throw new DocumentProcessingQueueFullException(
                _options.Capacity,
                TimeSpan.FromSeconds(
                    _options.EnqueueTimeoutSeconds));
        }
    }

    public async ValueTask<DocumentProcessingMessage> DequeueAsync(
        CancellationToken cancellationToken)
    {
        var message =
            await _queue.Reader.ReadAsync(
                cancellationToken);

        Interlocked.Decrement(
            ref _queueDepth);

        return message;
    }

    private static void ValidateOptions(
        DocumentProcessingQueueOptions options)
    {
        if (options.Capacity <= 0)
        {
            throw new InvalidOperationException(
                "Document processing queue capacity must be greater than zero.");
        }

        if (options.EnqueueTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Document processing queue enqueue timeout must be greater than zero.");
        }

        if (options.MaxConcurrentProcessors <= 0)
        {
            throw new InvalidOperationException(
                "MaxConcurrentProcessors must be greater than zero.");
        }
    }

    private static void ValidateMessage(
        DocumentProcessingMessage message)
    {
        if (message.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "DocumentId is required.",
                nameof(message));
        }

        if (string.IsNullOrWhiteSpace(
            message.CorrelationId))
        {
            throw new ArgumentException(
                "CorrelationId is required.",
                nameof(message));
        }

        if (string.IsNullOrWhiteSpace(
            message.QueuedByUserId))
        {
            throw new ArgumentException(
                "QueuedByUserId is required.",
                nameof(message));
        }

        if (message.QueuedAtUtc == default)
        {
            throw new ArgumentException(
                "QueuedAtUtc is required.",
                nameof(message));
        }
    }
}