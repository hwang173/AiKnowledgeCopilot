namespace AiKnowledgeCopilot.API.Models;

public class DevelopmentTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public string TokenType { get; init; } = "Bearer";

    public DateTime ExpiresAtUtc { get; init; }

    public string UserId { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}