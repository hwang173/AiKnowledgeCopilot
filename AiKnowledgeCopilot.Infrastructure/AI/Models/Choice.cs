using System.Text.Json.Serialization;

namespace AiKnowledgeCopilot.Infrastructure.AI.Models;

public class Choice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
        = new();
}