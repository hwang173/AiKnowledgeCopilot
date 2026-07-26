namespace AiKnowledgeCopilot.Infrastructure.AI;

public class EmbeddingCacheOptions
{
    public const string SectionName = "EmbeddingCache";

    public bool Enabled { get; set; } = true;

    public int ExpirationMinutes { get; set; } = 1440;

    public string KeyPrefix { get; set; } =
        "embedding";
}