using AiKnowledgeCopilot.Infrastructure.AI;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AiKnowledgeCopilot.API.HealthChecks;

public class OpenAiConfigurationHealthCheck : IHealthCheck
{
    private readonly OpenAiOptions _options;

    public OpenAiConfigurationHealthCheck(
        OpenAiOptions options)
    {
        _options =
            options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "OpenAI API key is not configured."));
        }

        if (!Uri.TryCreate(
                _options.BaseUrl,
                UriKind.Absolute,
                out _))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "OpenAI base URL is not a valid absolute URL."));
        }

        if (_options.RequestTimeoutSeconds <= 0)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "OpenAI request timeout must be greater than zero."));
        }

        if (_options.MaxRetryAttempts < 0)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "OpenAI max retry attempts cannot be negative."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                "OpenAI configuration is valid."));
    }
}