using AiKnowledgeCopilot.Application.Documents;

namespace AiKnowledgeCopilot.Infrastructure.Documents;

public class TextExtractionService : ITextExtractionService
{
    private readonly IReadOnlyCollection<ITextExtractor>
        _extractors;

    public TextExtractionService(
        IEnumerable<ITextExtractor> extractors)
    {
        _extractors =
            extractors.ToList();

        if (_extractors.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one text extractor must be registered.");
        }
    }

    public async Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "File path is required.",
                nameof(filePath));
        }

        var extractor =
            _extractors.FirstOrDefault(
                x => x.CanExtract(filePath));

        if (extractor is null)
        {
            throw new UnsupportedDocumentTypeException(
                filePath);
        }

        return await extractor.ExtractTextAsync(
            filePath,
            cancellationToken);
    }
}