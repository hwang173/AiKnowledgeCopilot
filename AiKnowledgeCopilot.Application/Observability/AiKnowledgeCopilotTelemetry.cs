using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AiKnowledgeCopilot.Application.Observability;

public static class AiKnowledgeCopilotTelemetry
{
    public const string ActivitySourceName =
        "AiKnowledgeCopilot";

    public const string MeterName =
        "AiKnowledgeCopilot";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);

    public static readonly Meter Meter =
        new(MeterName);

    public static readonly Counter<long> DocumentsQueuedCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.documents.queued",
            unit: "documents",
            description: "Total number of documents accepted into the processing queue.");

    public static readonly Histogram<double> DocumentQueueDelayHistogram =
        Meter.CreateHistogram<double>(
            name: "aiknowledgecopilot.document_processing.queue_delay",
            unit: "ms",
            description: "Time documents spend waiting in the processing queue.");

    public static readonly Histogram<double> DocumentProcessingDurationHistogram =
        Meter.CreateHistogram<double>(
            name: "aiknowledgecopilot.document_processing.duration",
            unit: "ms",
            description: "Time spent processing documents in background workers.");

    public static readonly Counter<long> DocumentProcessingSucceededCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.document_processing.succeeded",
            unit: "documents",
            description: "Total number of successfully processed documents.");

    public static readonly Counter<long> DocumentProcessingFailedCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.document_processing.failed",
            unit: "documents",
            description: "Total number of failed document processing attempts.");

    public static readonly Counter<long> OpenAiRequestCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.openai.requests",
            unit: "requests",
            description: "Total number of OpenAI HTTP requests.");

    public static readonly Counter<long> OpenAiRetryCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.openai.retries",
            unit: "retries",
            description: "Total number of OpenAI retry attempts.");

    public static readonly Counter<long> OpenAiFailureCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.openai.failures",
            unit: "failures",
            description: "Total number of final OpenAI request failures.");

    public static readonly Histogram<double> OpenAiRequestDurationHistogram =
        Meter.CreateHistogram<double>(
            name: "aiknowledgecopilot.openai.request_duration",
            unit: "ms",
            description: "Duration of OpenAI HTTP requests.");

    public static readonly Counter<long> EmbeddingCacheHitCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.embedding_cache.hits",
            unit: "hits",
            description: "Total number of embedding cache hits.");

    public static readonly Counter<long> EmbeddingCacheMissCounter =
        Meter.CreateCounter<long>(
            name: "aiknowledgecopilot.embedding_cache.misses",
            unit: "misses",
            description: "Total number of embedding cache misses.");
}