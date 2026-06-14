using AiKnowledgeCopilot.Application.Storage;
using Microsoft.AspNetCore.Http;

namespace AiKnowledgeCopilot.Infrastructure.Storage;

public class LocalFileStorageService
    : IFileStorageService
{
    public async Task<string> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var storageFolder =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "Storage");

        if (!Directory.Exists(storageFolder))
        {
            Directory.CreateDirectory(
                storageFolder);
        }

        var uniqueFileName =
            $"{Guid.NewGuid()}_{file.FileName}";

        var filePath =
            Path.Combine(
                storageFolder,
                uniqueFileName);

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        await file.CopyToAsync(
            stream,
            cancellationToken);

        return filePath;
    }

    public async Task<string> ReadTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(
            filePath,
            cancellationToken);
    }
}