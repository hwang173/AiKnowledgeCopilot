namespace AiKnowledgeCopilot.API.Models;

public class DevelopmentTokenRequest
{
    public string UserId { get; init; } = "user-1";

    public string DisplayName { get; init; } =
        "Development User";

    public string Role { get; init; } = "User";
}