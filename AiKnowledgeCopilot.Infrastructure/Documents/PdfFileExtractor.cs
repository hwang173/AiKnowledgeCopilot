using System.Text;
using UglyToad.PdfPig;

namespace AiKnowledgeCopilot.Infrastructure.Documents;

public class PdfFileExtractor : FileTextExtractorBase
{
    public override IReadOnlyCollection<string> SupportedExtensions =>
        [".pdf"];

    public override Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        EnsureFileExists(filePath);

        cancellationToken.ThrowIfCancellationRequested();

        var builder = new StringBuilder();

        using var document =
            PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(page.Text))
            {
                builder.AppendLine(page.Text);
                builder.AppendLine();
            }
        }

        return Task.FromResult(
            builder.ToString().Trim());
    }
}