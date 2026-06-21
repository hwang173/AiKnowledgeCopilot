using AiKnowledgeCopilot.Application.Documents;

namespace AiKnowledgeCopilot.Infrastructure.Documents;

public class SupportedDocumentTypesProvider
    : ISupportedDocumentTypesProvider
{
    private readonly IReadOnlyCollection<string>
        _supportedExtensions;

    public SupportedDocumentTypesProvider(
        IEnumerable<ITextExtractor> extractors)
    {
        _supportedExtensions =
            extractors
                .SelectMany(x => x.SupportedExtensions)
                .Select(NormalizeExtension)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
    }

    public IReadOnlyCollection<string> GetSupportedExtensions()
    {
        return _supportedExtensions;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        var normalizedExtension =
            extension.Trim();

        if (normalizedExtension.StartsWith('.'))
        {
            return normalizedExtension;
        }

        return $".{normalizedExtension}";
    }
}