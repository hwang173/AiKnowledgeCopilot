namespace AiKnowledgeCopilot.Application.AI;

public interface IEmbeddingCache
{
    Task<string?> GetAsync(
        string model,
        string text,
        CancellationToken cancellationToken);

    Task SetAsync(
        string model,
        string text,
        string embeddingJson,
        CancellationToken cancellationToken);
}