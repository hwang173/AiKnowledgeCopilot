namespace AiKnowledgeCopilot.API.Models;

public class UploadDocumentResponse
{
    public Guid DocumentId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string StatusUrl { get; init; } = string.Empty;
}