namespace SecureIncidentHub.Domain.Identity;

/// <summary>Exact OIDC issuer and subject, never email or display name.</summary>
public sealed record ExternalIdentityKey
{
    public ExternalIdentityKey(string issuer, string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        Issuer = issuer;
        Subject = subject;
    }

    public string Issuer { get; }
    public string Subject { get; }
}
