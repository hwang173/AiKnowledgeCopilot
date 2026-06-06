using AiKnowledgeCopilot.Domain.Entities;

namespace AiKnowledgeCopilot.Application.Repositories;

public interface IChunkRepository
{
    Task<List<Chunk>> GetAllAsync(
        CancellationToken cancellationToken);
}