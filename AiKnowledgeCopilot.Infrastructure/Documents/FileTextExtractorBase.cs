using AiKnowledgeCopilot.Application.Documents;

namespace AiKnowledgeCopilot.Infrastructure.Documents;

public abstract class FileTextExtractorBase : ITextExtractor
{
    public abstract IReadOnlyCollection<string> SupportedExtensions { get; }

    public bool CanExtract(string filePath)
    {
        var extension =
            Path.GetExtension(filePath);

        return SupportedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    public abstract Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken);

    protected static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Document file was not found.",
                filePath);
        }
    }
}