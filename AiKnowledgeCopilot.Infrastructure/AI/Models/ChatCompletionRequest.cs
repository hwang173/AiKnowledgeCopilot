using System.Text.Json.Serialization;

namespace AiKnowledgeCopilot.Infrastructure.AI.Models;

public class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
        = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; }
        = new();
}