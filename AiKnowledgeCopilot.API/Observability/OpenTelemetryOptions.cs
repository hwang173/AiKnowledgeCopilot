namespace AiKnowledgeCopilot.API.Observability;

public class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; set; } =
        "AiKnowledgeCopilot.Api";

    public string ServiceVersion { get; set; } =
        "1.0.0";

    public bool ConsoleExporterEnabled { get; set; } = true;

    public bool MetricsConsoleExporterEnabled { get; set; } = false;

    public bool OtlpExporterEnabled { get; set; } = false;

    public string OtlpEndpoint { get; set; } =
        "http://localhost:4317";

    public bool PrometheusExporterEnabled { get; set; } = true;

    public string PrometheusScrapingEndpoint { get; set; } =
        "/metrics";

    public double SamplingRatio { get; set; } = 1.0;
}