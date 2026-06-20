namespace AiKnowledgeCopilot.Application.Documents;

public interface ITextExtractor
{
    IReadOnlyCollection<string> SupportedExtensions { get; }

    bool CanExtract(string filePath);

    Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken);
}