# ADR 0009: SPA PKCE initially; BFF retained as a future option

Status: Accepted · 2026-09-14

## Context

Angular needs standards-based authentication without browser secrets. A BFF reduces token exposure but adds server sessions, cookies and CSRF obligations.

## Decision

Start with public-client Authorization Code Flow and PKCE S256, hosted ZITADEL login and memory-only access tokens. Implement state/nonce, exact redirects and narrowly scoped bearer requests with a reviewed library in Phase 1. No custom login page exists in Phase 0.

## Alternatives considered

BFF with HttpOnly cookies remains the main hardened option, requiring a separate ADR for sessions, CSRF, cookie scope and topology. Reject implicit/password flows and browser secrets.

## Consequences

Reload loses tokens; deliberate reauthentication is initially acceptable. Persistent refresh tokens and silent refresh are not assumed.

## Security implications

PKCE protects code exchange, not live tokens against XSS. Browser code is untrusted; authorize server-side. Production CSP, trusted dependencies and encoding are still required.

## Operational implications

Redirect origins and client registration must match each environment. BFF adoption changes trust boundaries and requires updated tests/threat model.
