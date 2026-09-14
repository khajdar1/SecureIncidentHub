using System.Diagnostics.Metrics;

namespace SecureIncidentHub.Worker;

/// <summary>Host lifecycle only. Message consumption begins with the first asynchronous use case.</summary>
public sealed partial class WorkerLifecycle(ILogger<WorkerLifecycle> logger) : IHostedService, IDisposable
{
    private readonly Meter meter = new("SecureIncidentHub.Worker");
    private int running;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        meter.CreateObservableGauge("hub.worker.running", () => Volatile.Read(ref running));
        Volatile.Write(ref running, 1);
        LogStarted(logger);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Volatile.Write(ref running, 0);
        LogStopped(logger);
        return Task.CompletedTask;
    }

    public void Dispose() => meter.Dispose();

    [LoggerMessage(Level = LogLevel.Information, Message = "Worker host started; no consumers registered in Phase 0")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Worker host stopped")]
    private static partial void LogStopped(ILogger logger);
}
