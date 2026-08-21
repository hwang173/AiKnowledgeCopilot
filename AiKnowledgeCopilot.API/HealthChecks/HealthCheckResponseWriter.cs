using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AiKnowledgeCopilot.API.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType =
            "application/json";

        var response =
            new
            {
                status = report.Status.ToString(),
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                entries = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        durationMs = entry.Value.Duration.TotalMilliseconds,
                        error = entry.Value.Exception?.Message
                    })
            };

        var json =
            JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        return context.Response.WriteAsync(
            json);
    }
}