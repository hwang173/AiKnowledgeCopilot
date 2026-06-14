namespace AiKnowledgeCopilot.Application.Documents;

public class UploadDocumentCommand
{
    public string FileName { get; set; }
        = string.Empty;

    public string FilePath { get; set; }
        = string.Empty;

    public string UploadedByUserId { get; set; }
        = string.Empty;
}