using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace SecureIncidentHub.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string Issuer = "https://identity.example";
    public const string Audience = "secure-incident-hub-api";
    private readonly RSA rsa = RSA.Create(2048);
    public RsaSecurityKey SigningKey => new(rsa) { KeyId = "ephemeral-test-key" };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Identity:Enabled", "true");
        builder.UseSetting("Identity:Authority", Issuer);
        builder.UseSetting("Identity:Audience", Audience);
        builder.UseSetting("Identity:RequiredScope", "incidents.api");
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IStartupFilter, TestRoutes>();
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var metadata = new OpenIdConnectConfiguration { Issuer = Issuer };
                metadata.SigningKeys.Add(SigningKey);
                options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(metadata);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) { rsa.Dispose(); }
    }

    private sealed class TestRoutes : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.UseEndpoints(endpoints =>
            {
                // Neither route is compiled into the production application.
                endpoints.MapGet("/_test/protected", () => Results.Ok());
                endpoints.MapGet("/_test/fault", (Func<IResult>)(() =>
                    throw new InvalidOperationException("sensitive-test-marker"))).AllowAnonymous();
            });
        };
    }
}
