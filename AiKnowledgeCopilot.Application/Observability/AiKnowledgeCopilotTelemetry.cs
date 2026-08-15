using System.Diagnostics;

namespace AiKnowledgeCopilot.Application.Observability;

public static class AiKnowledgeCopilotTelemetry
{
    public const string ActivitySourceName =
        "AiKnowledgeCopilot";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}