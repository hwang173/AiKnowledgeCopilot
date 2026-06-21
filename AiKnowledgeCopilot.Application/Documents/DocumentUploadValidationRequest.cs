namespace AiKnowledgeCopilot.Application.Documents;

public class DocumentUploadValidationRequest
{
    public string? FileName { get; init; }

    public long FileSizeInBytes { get; init; }

    public string? UploadedByUserId { get; init; }
}