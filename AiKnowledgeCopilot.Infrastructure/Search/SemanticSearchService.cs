using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using AiKnowledgeCopilot.Application.Repositories;

namespace AiKnowledgeCopilot.Infrastructure.Search;

public class SemanticSearchService
    : ISemanticSearchService
{
    private const double SimilarityThreshold = 0.75;

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
        var queryEmbeddingJson =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        var queryVector =
            EmbeddingParser.Parse(
                queryEmbeddingJson);

        var chunks =
            await _chunkRepository.GetAllAsync(
                cancellationToken);

        chunks = chunks
            .Where(x => x.Embedding != null)
            .ToList();

        var results = new List<SearchResultDto>();

        foreach (var chunk in chunks)
        {
            var chunkVector =
                EmbeddingParser.Parse(
                    chunk.Embedding!);

            var similarity =
                SimilarityCalculator
                    .CosineSimilarity(
                        queryVector,
                        chunkVector);
            var result =
                new SearchResultDto
                {
                    ChunkId = chunk.Id,

                    Content = chunk.Content,

                    Similarity = similarity
                };

            results.Add(result);
        }

        return results
            .Where(x =>
                x.Similarity >= SimilarityThreshold)
            .OrderByDescending(
                x => x.Similarity)
            .Take(5)
            .ToList();
    }
}