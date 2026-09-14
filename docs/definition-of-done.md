# Definition of Done

For each coherent slice:

- State acceptance criteria, inspect Git state and preserve existing changes.
- Enforce server-side validation, explicit tenant scope and failure behavior. Every tenant endpoint has same-tenant permitted/denied and cross-tenant tests, including stale membership/roles.
- Keep Domain/Application independent of providers and hosts; use meaningful adapters.
- Pass formatting, analyzers, builds and relevant unit/integration/security tests. Do not mask failed checks; record environmental blockers exactly.
- Review dependencies/licenses and locked versions; scan secrets and dependencies. Keep generated files, local env/state and credentials untracked.
- Provide safe Problem Details, useful redacted logs and adequate lifecycle/request/processing signals for the implemented behavior.
- Update relevant ADRs, diagrams, threat model, runbooks and learning log in the same commit as behavior changes.
- Review the complete diff and commit one verified Conventional Commit when requested. Do not push without authorization.
- Report verified outcomes, limitations, next step and what the owner should understand.

Phase 0 specifically requires runnable SPA/API/worker skeletons, only diagnostic API routes, offline identity defaults, object-storage contracts without an adapter, PostgreSQL/RabbitMQ configuration, resource-free Terraform and CI without cloud authentication. Container runtime verification and real ZITADEL/AWS authentication must be disclosed separately if unavailable; they are not inferred from configuration validation.
