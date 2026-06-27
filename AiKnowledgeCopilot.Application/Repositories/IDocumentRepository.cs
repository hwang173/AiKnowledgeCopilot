using AiKnowledgeCopilot.Domain.Entities;

namespace AiKnowledgeCopilot.Application.Repositories;

public interface IDocumentRepository
{
    Task AddAsync(
        Document document,
        CancellationToken cancellationToken);

    Task<Document?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Document?> GetByIdForUserAsync(
        Guid id,
        string uploadedByUserId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}