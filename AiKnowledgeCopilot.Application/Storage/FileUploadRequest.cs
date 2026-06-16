namespace AiKnowledgeCopilot.Application.Storage;

public class FileUploadRequest
{
    public string FileName { get; init; }
        = string.Empty;

    public Stream Content { get; init; }
        = Stream.Null;
}