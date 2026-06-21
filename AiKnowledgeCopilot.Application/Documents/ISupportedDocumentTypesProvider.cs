namespace AiKnowledgeCopilot.Application.Documents;

public interface ISupportedDocumentTypesProvider
{
    IReadOnlyCollection<string> GetSupportedExtensions();
}