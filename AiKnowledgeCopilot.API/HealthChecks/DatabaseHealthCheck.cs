using AiKnowledgeCopilot.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AiKnowledgeCopilot.API.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(
        AppDbContext dbContext)
    {
        _dbContext =
            dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect =
                await _dbContext.Database.CanConnectAsync(
                    cancellationToken);

            if (canConnect)
            {
                return HealthCheckResult.Healthy(
                    "PostgreSQL database is reachable.");
            }

            return HealthCheckResult.Unhealthy(
                "PostgreSQL database is not reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "PostgreSQL database health check failed.",
                ex);
        }
    }
}