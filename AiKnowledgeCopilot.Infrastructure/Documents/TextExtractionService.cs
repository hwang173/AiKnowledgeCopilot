using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Storage;

namespace AiKnowledgeCopilot.Infrastructure.Documents;

public class TextExtractionService
    : ITextExtractionService
{
    private readonly IFileStorageService
        _fileStorageService;

    public TextExtractionService(
        IFileStorageService fileStorageService)
    {
        _fileStorageService =
            fileStorageService;
    }

    public async Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        return await _fileStorageService
            .ReadTextAsync(
                filePath,
                cancellationToken);
    }
}