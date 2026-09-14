# ADR 0004: Application-owned authorization and tenant isolation

Status: Accepted · 2026-09-14

## Context

Authentication does not establish current membership or incident ownership. Token roles can outlive membership changes.

## Decision

PostgreSQL owns organizations, active memberships and roles. Resolve authenticated `(iss, sub)` and authorize every operation/resource server-side. Queries visibly include organization scope. Default endpoint policy denies anonymous access; only documented diagnostics opt out.

## Alternatives considered

Provider-owned roles reduce lookups but lose immediate revocation/application ownership. Database-per-tenant adds cost early. Row-level security may later supplement explicit scope.

## Consequences

No tenant endpoint ships without cross-tenant and membership-change tests. Phase 0 has no membership/role store.

## Security implications

Client organization IDs select candidates, not authority. Cross-organization privileges require exceptional explicit authorization and audit.

## Operational implications

Membership availability is security-critical. No authorization cache is introduced; any future cache must preserve an explicit revocation policy.
