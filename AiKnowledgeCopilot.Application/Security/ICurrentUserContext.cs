namespace AiKnowledgeCopilot.Application.Security;

public interface ICurrentUserContext
{
    string? UserId { get; }

    bool IsAuthenticated { get; }
}