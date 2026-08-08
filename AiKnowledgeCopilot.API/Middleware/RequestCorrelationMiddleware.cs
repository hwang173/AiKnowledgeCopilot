using AiKnowledgeCopilot.API.Observability;
using AiKnowledgeCopilot.Application.Security;
using Serilog.Context;

namespace AiKnowledgeCopilot.API.Middleware;

public class RequestCorrelationMiddleware
{
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

        httpContext.Items[HttpCorrelationContext.CorrelationIdItemKey] =
            correlationId;

        httpContext.Response.Headers[
            HttpCorrelationContext.CorrelationIdHeaderName] =
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
                HttpCorrelationContext.CorrelationIdHeaderName,
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