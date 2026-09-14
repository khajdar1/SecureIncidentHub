# Identity baseline

Create a ZITADEL Cloud public SPA application for Authorization Code Flow with PKCE S256, exact redirect/logout URIs and no client secret. Register the API audience/project and API scope. Configure JWT access tokens with the expected audience; opaque tokens and ID tokens are not API credentials.

The frontend identity example contains public configuration only. No login client consumes it in Phase 0. In Phase 1 select a maintained OIDC library, verify state/nonce, implement code exchange and retain tokens only in memory. Do not use localStorage/sessionStorage for tokens or arbitrary return URLs. Loopback HTTP is a development-only redirect exception; deployed traffic requires HTTPS.

API settings are `Identity:Enabled`, `Authority`, `Audience` and `RequiredScope`. Identity defaults off for offline startup. Enabling it validates configuration, requires HTTPS metadata, preserves claim names, validates issuer/audience/signature/expiry/not-before and restricts authentication to Bearer. The fallback policy requires authentication and an exact space-delimited API scope. Three diagnostic routes explicitly allow anonymous access. CORS is not enabled because the SPA makes no API calls yet.

Disabled identity does not create a fake user or trust headers: protected routes remain denied. Invalid/missing credentials return safe 401; a valid identity missing scope gets 403. Test-only routes exercise this using ephemeral signed tokens and local validation parameters. Real discovery, provider audience settings and key rotation are Phase 1 checks.

External users are keyed by the exact, case-sensitive `(iss, sub)` pair. Never normalize signed identity claims or use email as the permanent key. PostgreSQL will own active memberships and roles; removed membership and changed roles must affect existing sessions. No membership store or tenant endpoint exists yet.

Before any tenant endpoint, verify permitted/denied same-tenant roles, cross-tenant reads/updates, ID manipulation, evidence access and membership/role changes with an existing token. Constraints must carry tenant ownership through relationships. Row-level security can later add defense after explicit scoped queries work.

References: [ZITADEL](https://zitadel.com/docs/guides/integrate/login/oidc/login-users), [Microsoft JWT bearer](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0).
