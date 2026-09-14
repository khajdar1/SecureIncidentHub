using SecureIncidentHub.Infrastructure.Observability;
using SecureIncidentHub.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
builder.Services.AddHubTelemetry(builder.Configuration, "SecureIncidentHub.Worker", isApi: false);
builder.Services.AddHostedService<WorkerLifecycle>();
await builder.Build().RunAsync();
