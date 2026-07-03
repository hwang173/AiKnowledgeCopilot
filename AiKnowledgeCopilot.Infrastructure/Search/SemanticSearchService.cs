using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Search;

namespace AiKnowledgeCopilot.Infrastructure.Search;

public class SemanticSearchService
    : ISemanticSearchService
{
    private const double SimilarityThreshold = 0.5;

    private const int MaxResults = 5;

    private readonly IEmbeddingService _embeddingService;

    private readonly IChunkRepository _chunkRepository;

    public SemanticSearchService(
        IChunkRepository chunkRepository,
        IEmbeddingService embeddingService)
    {
        _chunkRepository =
            chunkRepository;

        _embeddingService =
            embeddingService;
    }

    public async Task<List<SearchResultDto>> SearchAsync(
        SemanticSearchQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var queryVector =
            await GenerateQueryVectorAsync(
                query.Query,
                cancellationToken);

        var chunks =
            await GetSearchableChunksAsync(
                query.RequestedByUserId,
                cancellationToken);

        var results =
            new List<SearchResultDto>();

        foreach (var chunk in chunks)
        {
            var similarity =
                CalculateSimilarity(
                    queryVector,
                    chunk.Embedding!);

            var result =
                CreateSearchResult(
                    chunk,
                    similarity);

            results.Add(result);
        }

        return RankResults(results);
    }

    private static void ValidateQuery(
        SemanticSearchQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            throw new ArgumentException(
                "Search query is required.",
                nameof(query));
        }

        if (string.IsNullOrWhiteSpace(
            query.RequestedByUserId))
        {
            throw new ArgumentException(
                "RequestedByUserId is required.",
                nameof(query));
        }
    }

    private async Task<float[]> GenerateQueryVectorAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var queryEmbeddingJson =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        return EmbeddingParser.Parse(
            queryEmbeddingJson);
    }

    private async Task<List<Domain.Entities.Chunk>>
        GetSearchableChunksAsync(
            string requestedByUserId,
            CancellationToken cancellationToken)
    {
        return await _chunkRepository
            .GetSearchableChunksForUserAsync(
                requestedByUserId,
                cancellationToken);
    }

    private static SearchResultDto CreateSearchResult(
        Domain.Entities.Chunk chunk,
        double similarity)
    {
        return new SearchResultDto
        {
            ChunkId = chunk.Id,

            DocumentId = chunk.DocumentId,

            Content = chunk.Content,

            Similarity = similarity
        };
    }

    private static double CalculateSimilarity(
        float[] queryVector,
        string embeddingJson)
    {
        var chunkVector =
            EmbeddingParser.Parse(
                embeddingJson);

        return SimilarityCalculator
            .CosineSimilarity(
                queryVector,
                chunkVector);
    }

    private static List<SearchResultDto> RankResults(
        List<SearchResultDto> results)
    {
        return results
            .Where(x =>
                x.Similarity >= SimilarityThreshold)
            .OrderByDescending(
                x => x.Similarity)
            .Take(MaxResults)
            .ToList();
    }
}