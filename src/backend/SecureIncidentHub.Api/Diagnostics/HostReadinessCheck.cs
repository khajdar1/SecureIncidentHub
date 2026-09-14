using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SecureIncidentHub.Api.Diagnostics;

public sealed class HostReadinessCheck(IHostApplicationLifetime lifetime) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default) => Task.FromResult(
        lifetime.ApplicationStarted.IsCancellationRequested && !lifetime.ApplicationStopping.IsCancellationRequested
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy());
}
