using System.Text.Json.Serialization;

namespace AiKnowledgeCopilot.Infrastructure.AI.Models;

public class ChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; }
        = new();
}