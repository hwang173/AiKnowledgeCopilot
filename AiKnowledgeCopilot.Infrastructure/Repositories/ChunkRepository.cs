using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Domain.Entities;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiKnowledgeCopilot.Infrastructure.Repositories;

public class ChunkRepository
    : IChunkRepository
{
    private readonly AppDbContext _dbContext;

    public ChunkRepository(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Chunk>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Chunks
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}