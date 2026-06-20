namespace AiKnowledgeCopilot.Application.Documents;

public class UnsupportedDocumentTypeException : Exception
{
    public UnsupportedDocumentTypeException(string filePath)
        : base($"Unsupported document type: {Path.GetExtension(filePath)}")
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}