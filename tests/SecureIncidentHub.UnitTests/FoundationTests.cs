using System.Xml.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SecureIncidentHub.Api.Diagnostics;
using SecureIncidentHub.Api.Identity;
using SecureIncidentHub.Domain.Identity;
using SecureIncidentHub.Worker;

namespace SecureIncidentHub.UnitTests;

public sealed class FoundationTests
{
    [Fact]
    public void IdentityUsesExactIssuerAndSubject()
    {
        var key = new ExternalIdentityKey("https://identity.example", "subject-A");
        Assert.Equal(key, new ExternalIdentityKey("https://identity.example", "subject-A"));
        Assert.NotEqual(key, new ExternalIdentityKey("https://other.example", "subject-A"));
        Assert.NotEqual(key, new ExternalIdentityKey("https://identity.example", "subject-a"));
        Assert.NotEqual(key, new ExternalIdentityKey("https://identity.example/", "subject-A"));
    }

    [Theory]
    [InlineData("", "subject")]
    [InlineData("issuer", " ")]
    public void IdentityRejectsMissingParts(string issuer, string subject) =>
        Assert.Throws<ArgumentException>(() => new ExternalIdentityKey(issuer, subject));

    [Theory]
    [InlineData("http://identity.example", "api", "incidents.api")]
    [InlineData("https://user@identity.example", "api", "incidents.api")]
    [InlineData("https://identity.example?query=value", "api", "incidents.api")]
    [InlineData("https://identity.example", "", "incidents.api")]
    [InlineData("https://identity.example", "api", "scope other")]
    public void EnabledIdentityRejectsUnsafeConfiguration(string authority, string audience, string scope) =>
        Assert.False(new IdentityOptions
        {
            Enabled = true,
            Authority = authority,
            Audience = audience,
            RequiredScope = scope
        }.IsValid());

    [Fact]
    public async Task ReadinessTracksStartupAndStopping()
    {
        using var lifetime = new TestLifetime();
        var check = new HostReadinessCheck(lifetime);
        Assert.Equal(HealthStatus.Unhealthy, (await check.CheckHealthAsync(new())).Status);
        lifetime.Started.Cancel();
        Assert.Equal(HealthStatus.Healthy, (await check.CheckHealthAsync(new())).Status);
        lifetime.StopApplication();
        Assert.Equal(HealthStatus.Unhealthy, (await check.CheckHealthAsync(new())).Status);
    }

    [Fact]
    public async Task WorkerStartsAndStopsWithoutInfrastructure()
    {
        using var worker = new WorkerLifecycle(NullLogger<WorkerLifecycle>.Instance);
        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void ProductionProjectGraphPreservesDependencyDirection()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Directory.Build.props")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        var expected = new Dictionary<string, string[]>
        {
            ["Domain"] = [],
            ["Application"] = ["Domain"],
            ["Infrastructure"] = ["Application"],
            ["Api"] = ["Infrastructure"],
            ["Worker"] = ["Infrastructure"]
        };
        var projects = Directory.GetFiles(Path.Combine(root.FullName, "src", "backend"), "*.csproj", SearchOption.AllDirectories);
        Assert.Equal(expected.Count, projects.Length);
        foreach (var file in projects)
        {
            var name = Path.GetFileNameWithoutExtension(file).Replace("SecureIncidentHub.", "", StringComparison.Ordinal);
            var project = XDocument.Load(file);
            var references = project.Descendants("ProjectReference").Select(element =>
                Path.GetFileNameWithoutExtension(element.Attribute("Include")!.Value)
                    .Replace("SecureIncidentHub.", "", StringComparison.Ordinal)).Order().ToArray();
            Assert.Equal(expected[name].Order(), references);
            if (name is "Domain" or "Application")
            {
                Assert.Empty(project.Descendants("PackageReference"));
                Assert.Empty(project.Descendants("FrameworkReference"));
                Assert.Equal("Microsoft.NET.Sdk", project.Root!.Attribute("Sdk")!.Value);
            }
        }
    }

    private sealed class TestLifetime : IHostApplicationLifetime, IDisposable
    {
        public CancellationTokenSource Started { get; } = new();
        private readonly CancellationTokenSource stopping = new();
        public CancellationToken ApplicationStarted => Started.Token;
        public CancellationToken ApplicationStopping => stopping.Token;
        public CancellationToken ApplicationStopped => CancellationToken.None;
        public void StopApplication() => stopping.Cancel();
        public void Dispose() { Started.Dispose(); stopping.Dispose(); }
    }
}
