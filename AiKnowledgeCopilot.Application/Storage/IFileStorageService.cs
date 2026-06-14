using Microsoft.AspNetCore.Http;

namespace AiKnowledgeCopilot.Application.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task<string> ReadTextAsync(
        string filePath,
        CancellationToken cancellationToken);
}