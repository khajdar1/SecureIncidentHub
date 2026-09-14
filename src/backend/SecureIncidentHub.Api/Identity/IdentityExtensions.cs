using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace SecureIncidentHub.Api.Identity;

public static class IdentityExtensions
{
    public static IServiceCollection AddHubIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var identity = configuration.GetSection("Identity").Get<IdentityOptions>() ?? new IdentityOptions();
        services.AddOptions<IdentityOptions>().BindConfiguration("Identity")
            .Validate(options => options.IsValid(), "Enabled identity requires an HTTPS authority, audience and single API scope.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.RequireHttpsMetadata = true;
            options.IncludeErrorDetails = false;
            options.SaveToken = false;
            if (identity.Enabled)
            {
                options.Authority = identity.Authority;
                options.Audience = identity.Audience;
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = identity.Authority,
                ValidateAudience = true,
                ValidAudience = identity.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ClockSkew = TimeSpan.FromSeconds(30)
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (!identity.Enabled)
                    {
                        context.NoResult();
                    }

                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorizationBuilder().SetFallbackPolicy(
            new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim("iss")
                .RequireClaim("sub")
                .RequireAssertion(context => identity.Enabled && context.User.FindAll("scope")
                    .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Contains(identity.RequiredScope, StringComparer.Ordinal))
                .Build());
        return services;
    }
}
