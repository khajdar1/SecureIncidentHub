using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SecureIncidentHub.IntegrationTests;

public sealed class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string[] DiagnosticRoutes = ["/examples/problem", "/health/live", "/health/ready"];
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthIsAnonymousAndRevealsOnlyStatus(string path)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task ExampleUsesSafeProblemDetailsAndServerCorrelation()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "untrusted-client-value");
        using var response = await client.GetAsync("/examples/problem?secret=sensitive-test-marker");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), json.RootElement.GetProperty("traceId").GetString());
        Assert.DoesNotContain("untrusted-client-value", body, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-test-marker", body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Development", "application/json")]
    [InlineData("Production", "application/json")]
    [InlineData("Development", "text/html")]
    [InlineData("Production", "text/html")]
    public async Task ExceptionsRemainSafeInEveryEnvironment(string environment, string accept)
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var client = configured.CreateClient();
        client.DefaultRequestHeaders.Accept.ParseAdd(accept);
        using var response = await client.GetAsync("/_test/fault");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("sensitive-test-marker", body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("valid", HttpStatusCode.OK)]
    [InlineData("missing", HttpStatusCode.Unauthorized)]
    [InlineData("malformed", HttpStatusCode.Unauthorized)]
    [InlineData("issuer", HttpStatusCode.Unauthorized)]
    [InlineData("audience", HttpStatusCode.Unauthorized)]
    [InlineData("expired", HttpStatusCode.Unauthorized)]
    [InlineData("future", HttpStatusCode.Unauthorized)]
    [InlineData("signature", HttpStatusCode.Unauthorized)]
    [InlineData("unsigned", HttpStatusCode.Unauthorized)]
    [InlineData("no-expiry", HttpStatusCode.Unauthorized)]
    [InlineData("scheme", HttpStatusCode.Unauthorized)]
    [InlineData("scope", HttpStatusCode.Forbidden)]
    [InlineData("scope-prefix", HttpStatusCode.Forbidden)]
    [InlineData("subject", HttpStatusCode.Forbidden)]
    public async Task DefaultPolicyValidatesRealSignedTokens(string variation, HttpStatusCode expected)
    {
        using var client = factory.CreateClient();
        using var wrongRsa = RSA.Create(2048);
        if (variation != "missing")
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                variation == "scheme" ? "Basic" : "Bearer",
                variation == "malformed" ? "not-a-jwt" : CreateToken(variation, wrongRsa));
        }

        using var response = await client.GetAsync("/_test/protected");
        Assert.Equal(expected, response.StatusCode);
        if (expected is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
            Assert.DoesNotContain("IDX", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task DisabledIdentityDoesNotPermitProtectedRequests()
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.UseSetting("Identity:Enabled", "false"));
        using var client = configured.CreateClient();
        using var rsa = RSA.Create(2048);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken("valid", rsa));
        using var response = await client.GetAsync("/_test/protected");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void EnabledInvalidIdentityFailsStartup()
    {
        using var configured = factory.WithWebHostBuilder(builder => builder.UseSetting("Identity:Authority", "http://identity.example"));
        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() => configured.CreateClient());
    }

    [Fact]
    public async Task ProductionHostHasOnlyThreeDiagnosticRoutesAndStartsOffline()
    {
        using var clean = new WebApplicationFactory<Program>();
        using var client = clean.CreateClient();
        using var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var routes = clean.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>().Select(endpoint => endpoint.RoutePattern.RawText).Order().ToArray();
        Assert.Equal(DiagnosticRoutes, routes);
    }

    private string CreateToken(string variation, RSA wrongRsa)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>();
        if (variation != "subject") { claims.Add(new("sub", "test-subject")); }
        claims.Add(new("scope", variation switch
        {
            "scope" => "openid",
            "scope-prefix" => "incidents.api.extra",
            _ => "openid incidents.api"
        }));
        var token = new JwtSecurityToken(
            variation == "issuer" ? "https://other.example" : ApiFactory.Issuer,
            variation == "audience" ? "other-api" : ApiFactory.Audience,
            claims,
            variation == "future" ? now.AddMinutes(10) : now.AddMinutes(-20),
            variation == "expired" ? now.AddMinutes(-10) : now.AddMinutes(20),
            variation == "unsigned" ? null : new SigningCredentials(
                variation == "signature" ? new RsaSecurityKey(wrongRsa) : factory.SigningKey,
                SecurityAlgorithms.RsaSha256));
        if (variation == "no-expiry") { token.Payload.Remove("exp"); }
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
