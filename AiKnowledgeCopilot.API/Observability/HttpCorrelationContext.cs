using AiKnowledgeCopilot.Application.Observability;

namespace AiKnowledgeCopilot.API.Observability;

public class HttpCorrelationContext : ICorrelationContext
{
    public const string CorrelationIdHeaderName =
        "X-Correlation-Id";

    public const string CorrelationIdItemKey =
        "CorrelationId";

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public HttpCorrelationContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    public string CorrelationId
    {
        get
        {
            var httpContext =
                _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return Guid.NewGuid().ToString("N");
            }

            if (httpContext.Items.TryGetValue(
                    CorrelationIdItemKey,
                    out var correlationIdValue) &&
                correlationIdValue is string correlationId &&
                !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId;
            }

            if (httpContext.Request.Headers.TryGetValue(
                    CorrelationIdHeaderName,
                    out var correlationIdValues))
            {
                var headerCorrelationId =
                    correlationIdValues.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(headerCorrelationId))
                {
                    return headerCorrelationId.Trim();
                }
            }

            return Guid.NewGuid().ToString("N");
        }
    }
}