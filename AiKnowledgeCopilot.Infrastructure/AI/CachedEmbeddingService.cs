using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Observability;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class CachedEmbeddingService : IEmbeddingService
{
    private readonly OpenAiEmbeddingService _innerService;

    private readonly IEmbeddingCache _embeddingCache;

    private readonly OpenAiOptions _options;

    private readonly ILogger<CachedEmbeddingService> _logger;

    public CachedEmbeddingService(
        OpenAiEmbeddingService innerService,
        IEmbeddingCache embeddingCache,
        OpenAiOptions options,
        ILogger<CachedEmbeddingService> logger)
    {
        _innerService = innerService;

        _embeddingCache = embeddingCache;

        _options = options;

        _logger = logger;
    }

    public async Task<string> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var cachedEmbedding =
            await _embeddingCache.GetAsync(
                _options.EmbeddingModel,
                text,
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(cachedEmbedding))
        {
            AiKnowledgeCopilotTelemetry.EmbeddingCacheHitCounter.Add(
                1,
                new KeyValuePair<string, object?>(
                    "model",
                    _options.EmbeddingModel));

            _logger.LogInformation(
                "Embedding cache hit for model {Model}.",
                _options.EmbeddingModel);

            return cachedEmbedding;
        }

        AiKnowledgeCopilotTelemetry.EmbeddingCacheMissCounter.Add(
            1,
            new KeyValuePair<string, object?>(
                "model",
                _options.EmbeddingModel));

        _logger.LogInformation(
            "Embedding cache miss for model {Model}.",
            _options.EmbeddingModel);

        var embedding =
            await _innerService.GenerateEmbeddingAsync(
                text,
                cancellationToken);

        await _embeddingCache.SetAsync(
            _options.EmbeddingModel,
            text,
            embedding,
            cancellationToken);

        return embedding;
    }
}