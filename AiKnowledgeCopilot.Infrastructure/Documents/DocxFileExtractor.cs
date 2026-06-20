using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AiKnowledgeCopilot.Infrastructure.Documents;

public class DocxFileExtractor : FileTextExtractorBase
{
    public override IReadOnlyCollection<string> SupportedExtensions =>
        [".docx"];

    public override Task<string> ExtractTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        EnsureFileExists(filePath);

        cancellationToken.ThrowIfCancellationRequested();

        var builder = new StringBuilder();

        using var document =
            WordprocessingDocument.Open(
                filePath,
                false);

        var body =
            document.MainDocumentPart?
                .Document?
                .Body;

        if (body is null)
        {
            return Task.FromResult(string.Empty);
        }

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text =
                string.Concat(
                    paragraph
                        .Descendants<Text>()
                        .Select(x => x.Text));

            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
            }
        }

        return Task.FromResult(
            builder.ToString().Trim());
    }
}