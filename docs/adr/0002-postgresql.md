# ADR 0002: PostgreSQL as the system of record

Status: Accepted · 2026-09-14

## Context

Membership, incident transitions, evidence metadata and audit intent require transactions and relational constraints.

## Decision

Use supported PostgreSQL 17 with maintenance patches, locally in Compose and eventually RDS. Object storage owns evidence bytes. Introduce deterministic migrations and direct persistence with the first real use case.

## Alternatives considered

A document database makes relationships harder to verify. SQLite cannot demonstrate PostgreSQL constraint/isolation behavior.

## Consequences

Use disposable PostgreSQL for future persistence tests. Phase 0 introduces no ORM, schema, migration or seed data.

## Security implications

Tenant-owned rows and relevant foreign keys must include organization scope. Separate migration and application privileges before deployment.

## Operational implications

Backups, restoration tests and patching become mandatory when data exists. Local volumes are disposable, not production backups.
