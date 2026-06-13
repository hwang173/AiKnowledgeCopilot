using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using AiKnowledgeCopilot.Application.Repositories;

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
        string query,
        CancellationToken cancellationToken)
    {
        var queryVector =
            await GenerateQueryVectorAsync(
                query,
                cancellationToken);

        var chunks =
            await GetSearchableChunksAsync(
                cancellationToken);

        var results = new List<SearchResultDto>();

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
        CancellationToken cancellationToken)
    {
        var chunks =
            await _chunkRepository.GetAllAsync(
                cancellationToken);

        return chunks
            .Where(x => x.Embedding != null)
            .ToList();
    }

    private SearchResultDto CreateSearchResult(
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

    private double CalculateSimilarity(
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

    private List<SearchResultDto> RankResults(
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