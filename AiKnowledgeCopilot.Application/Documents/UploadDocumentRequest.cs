namespace AiKnowledgeCopilot.Application.Documents;

public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;

    public string UploadedByUserId { get; set; } = string.Empty;
}