# ADR 0003: ZITADEL Cloud through standard OIDC

Status: Accepted · 2026-09-14

## Context

Authentication and MFA should not require custom password handling or couple application rules to AWS IAM.

## Decision

Use external ZITADEL Cloud through OIDC/OAuth standards and Microsoft JWT bearer validation. Key users by exact `(iss, sub)`. Prepare configuration with no live credentials and no local identity container.

## Alternatives considered

Self-hosted identity adds operational responsibility. Cognito is viable through OIDC but not the selected provider. Custom authentication is outside scope.

## Consequences

Real discovery, rotation and audience settings need Phase 1 provider testing. No browser OIDC library is installed before a login use case.

## Security implications

Require signed access tokens, HTTPS metadata, valid issuer/audience/lifetime/scope and Bearer scheme. Do not use email as identity or log tokens.

## Operational implications

ZITADEL availability is external. Offline tests use ephemeral signing keys and explicit test validation parameters.
