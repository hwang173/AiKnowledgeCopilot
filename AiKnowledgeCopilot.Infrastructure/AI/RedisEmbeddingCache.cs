using System.Security.Cryptography;
using System.Text;
using AiKnowledgeCopilot.Application.AI;
using Microsoft.Extensions.Caching.Distributed;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class RedisEmbeddingCache : IEmbeddingCache
{
    private readonly IDistributedCache _cache;

    private readonly EmbeddingCacheOptions _options;

    public RedisEmbeddingCache(
        IDistributedCache cache,
        EmbeddingCacheOptions options)
    {
        _cache = cache;

        _options = options;
    }

    public async Task<string?> GetAsync(
        string model,
        string text,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var cacheKey =
            CreateCacheKey(
                model,
                text);

        return await _cache.GetStringAsync(
            cacheKey,
            cancellationToken);
    }

    public async Task SetAsync(
        string model,
        string text,
        string embeddingJson,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var cacheKey =
            CreateCacheKey(
                model,
                text);

        var cacheOptions =
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(
                        _options.ExpirationMinutes)
            };

        await _cache.SetStringAsync(
            cacheKey,
            embeddingJson,
            cacheOptions,
            cancellationToken);
    }

    private string CreateCacheKey(
        string model,
        string text)
    {
        var normalizedText =
            text.Trim();

        var rawKey =
            $"{model}:{normalizedText}";

        var hashBytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(rawKey));

        var hash =
            Convert
                .ToHexString(hashBytes)
                .ToLowerInvariant();

        return $"{_options.KeyPrefix}:{hash}";
    }
}
