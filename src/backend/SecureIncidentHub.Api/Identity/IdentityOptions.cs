namespace SecureIncidentHub.Api.Identity;

public sealed class IdentityOptions
{
    public bool Enabled { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string RequiredScope { get; set; } = string.Empty;

    public bool IsValid() => !Enabled ||
        (Uri.TryCreate(Authority, UriKind.Absolute, out var uri) &&
         uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.UserInfo) &&
         string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment) &&
         !string.IsNullOrWhiteSpace(Audience) && !string.IsNullOrWhiteSpace(RequiredScope) &&
         !RequiredScope.Any(char.IsWhiteSpace));
}
