using System.Security.Claims;
using AiKnowledgeCopilot.Application.Security;

namespace AiKnowledgeCopilot.API.Security;

public class JwtCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public JwtCurrentUserContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            var user =
                _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userId =
                user.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return userId.Trim();
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor
            .HttpContext?
            .User?
            .Identity?
            .IsAuthenticated == true;
}