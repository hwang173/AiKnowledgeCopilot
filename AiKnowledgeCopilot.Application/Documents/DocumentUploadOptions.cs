namespace AiKnowledgeCopilot.Application.Documents;

public class DocumentUploadOptions
{
    public const string SectionName = "DocumentUpload";

    public long MaxFileSizeInBytes { get; set; } =
        10 * 1024 * 1024;

    public int MaxFileNameLength { get; set; } = 500;

    public int MaxUploadedByUserIdLength { get; set; } = 100;
}