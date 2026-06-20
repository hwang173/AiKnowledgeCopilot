namespace AiKnowledgeCopilot.Infrastructure.Documents;

public class TextFileExtractor : FileTextExtractorBase
{
    public override IReadOnlyCollection<string> SupportedExtensions =>
        [".txt"];

    public override async Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        EnsureFileExists(filePath);

        return await File.ReadAllTextAsync(
            filePath,
            cancellationToken);
    }
}