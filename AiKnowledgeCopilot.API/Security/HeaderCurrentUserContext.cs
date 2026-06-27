using AiKnowledgeCopilot.Application.Security;

namespace AiKnowledgeCopilot.API.Security;

public class HeaderCurrentUserContext : ICurrentUserContext
{
    public const string UserIdHeaderName = "X-User-Id";

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public HeaderCurrentUserContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            var httpContext =
                _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            if (!httpContext.Request.Headers.TryGetValue(
                UserIdHeaderName,
                out var userIdValues))
            {
                return null;
            }

            var userId =
                userIdValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return userId.Trim();
        }
    }

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(UserId);
}