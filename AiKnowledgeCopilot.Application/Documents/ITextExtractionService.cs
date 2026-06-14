namespace AiKnowledgeCopilot.Application.Documents;

public interface ITextExtractionService
{
    Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken);
}