using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SecureIncidentHub.Api.Diagnostics;
using SecureIncidentHub.Api.Identity;
using SecureIncidentHub.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
builder.Services.AddHubIdentity(builder.Configuration);
builder.Services.AddHubTelemetry(builder.Configuration, "SecureIncidentHub.Api", isApi: true);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Extensions["traceId"] =
        Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
});
builder.Services.AddExceptionHandler<SafeExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<HostReadinessCheck>("host", tags: ["ready"]);

var app = builder.Build();
app.Use(async (context, next) =>
{
    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    context.Response.Headers["X-Correlation-ID"] = traceId;
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers.CacheControl = "no-store";
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = traceId }))
    {
        await next(context);
    }
});
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();
app.MapGet("/examples/problem", () => Results.Problem(
    statusCode: StatusCodes.Status400BadRequest,
    title: "Example validation error",
    detail: "This endpoint demonstrates the API error format."))
    .AllowAnonymous();
app.Run();

public partial class Program;
