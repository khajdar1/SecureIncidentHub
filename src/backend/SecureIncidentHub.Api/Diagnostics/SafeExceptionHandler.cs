using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace SecureIncidentHub.Api.Diagnostics;

public sealed partial class SafeExceptionHandler(
    IProblemDetailsService problemDetails, ILogger<SafeExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        // Exception messages/data may contain secrets. Log only safe diagnostic metadata once.
        LogFailure(logger, exception.GetType().Name, traceId);
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var problemContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Contact support with the correlation identifier."
            }
        };
        problemContext.ProblemDetails.Extensions["traceId"] = traceId;
        if (!await problemDetails.TryWriteAsync(problemContext))
        {
            // An unsupported Accept header must never trigger a developer exception page.
            await httpContext.Response.WriteAsJsonAsync(problemContext.ProblemDetails,
                options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        }

        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled request failure {ExceptionType}; correlation {TraceId}")]
    private static partial void LogFailure(ILogger logger, string exceptionType, string traceId);
}
