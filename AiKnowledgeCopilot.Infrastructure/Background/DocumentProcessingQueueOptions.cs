namespace AiKnowledgeCopilot.Infrastructure.Background;

public class DocumentProcessingQueueOptions
{
    public const string SectionName = "DocumentProcessingQueue";

    public int Capacity { get; set; } = 100;

    public int EnqueueTimeoutSeconds { get; set; } = 5;
}