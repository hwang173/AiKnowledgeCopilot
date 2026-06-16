namespace AiKnowledgeCopilot.Application.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken);

    Task<string> ReadTextAsync(
        string filePath,
        CancellationToken cancellationToken);
}