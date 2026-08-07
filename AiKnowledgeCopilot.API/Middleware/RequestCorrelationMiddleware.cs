using AiKnowledgeCopilot.Application.Security;
using Serilog.Context;

namespace AiKnowledgeCopilot.API.Middleware;

public class RequestCorrelationMiddleware
{
    public const string CorrelationIdHeaderName =
        "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public RequestCorrelationMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        ICurrentUserContext currentUserContext)
    {
        var correlationId =
            GetOrCreateCorrelationId(httpContext);

        httpContext.Response.Headers[CorrelationIdHeaderName] =
            correlationId;

        using (LogContext.PushProperty(
                   "CorrelationId",
                   correlationId))
        using (LogContext.PushProperty(
                   "RequestMethod",
                   httpContext.Request.Method))
        using (LogContext.PushProperty(
                   "RequestPath",
                   httpContext.Request.Path.Value))
        using (LogContext.PushProperty(
                   "UserId",
                   currentUserContext.UserId ?? "anonymous"))
        {
            await _next(httpContext);
        }
    }

    private static string GetOrCreateCorrelationId(
        HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue(
                CorrelationIdHeaderName,
                out var correlationIdValues))
        {
            var correlationId =
                correlationIdValues.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.Trim();
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}