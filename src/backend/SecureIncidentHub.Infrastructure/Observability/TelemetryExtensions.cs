using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SecureIncidentHub.Infrastructure.Observability;

public static class TelemetryExtensions
{
    public static IServiceCollection AddHubTelemetry(
        this IServiceCollection services, IConfiguration configuration, string serviceName, bool isApi)
    {
        var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var telemetry = services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService(serviceName));
        telemetry.WithTracing(tracing =>
        {
            tracing.AddSource(serviceName);
            if (isApi)
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = false;
                    options.EnrichWithHttpResponse = (activity, _) =>
                    {
                        // These hosts never need URLs or query values in exported telemetry.
                        activity.SetTag("url.query", null);
                        activity.SetTag("url.full", null);
                    };
                });
            }

            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                tracing.AddOtlpExporter();
            }
        });
        telemetry.WithMetrics(metrics =>
        {
            metrics.AddMeter(serviceName);
            if (isApi)
            {
                metrics.AddAspNetCoreInstrumentation();
            }

            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                metrics.AddOtlpExporter();
            }
        });
        return services;
    }
}
