using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Domain.Entities;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiKnowledgeCopilot.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public DocumentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        await _dbContext.Documents.AddAsync(
            document,
            cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Documents
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}