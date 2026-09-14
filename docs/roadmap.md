# Delivery roadmap

| Phase | Smallest useful outcome                       | Required exit evidence                                                                                                                                                             |
| ----- | --------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 0     | Repository/architecture foundation            | Offline builds/tests, local configuration validation, CI/security gates, reviewed commits                                                                                          |
| 1     | Identity and organization authorization slice | ZITADEL PKCE + API audience/scope validation; PostgreSQL identity/membership schema; protected current-membership endpoint; removed-membership/changed-role and cross-tenant tests |
| 2     | Incident reporting/review                     | Explicit lifecycle, validation, pagination, audit intent and tenant tests on every endpoint                                                                                        |
| 3     | Evidence upload/download                      | Reviewed supported local storage candidates and owner decision, real adapter contract tests, private storage, grants, size/type/hash/ownership/expiry checks                       |
| 4     | Background processing and notifications       | Transactional outbox, durable delivery, idempotency, retry/dead-letter/replay tests, processing signals                                                                            |
| 5     | First cost-conscious AWS lab deployment       | Explicit authorization, account/service preflight, budget controls, protected state, resource/exposure/cost/cleanup review, temporary local credentials                            |
| 6     | Production-reference hardening                | Restore/rollback exercises, operational SLOs, deeper security/accessibility tests and documented residual risk                                                                     |

Track phase identifiers in implementation notes. No broad TODO is permission to implement later phases. Phase 0 stops before live login or domain workflows. The first recommended Phase 1 task is the organization membership authorization slice, using real disposable PostgreSQL and fake signing keys in CI before verifying a real ZITADEL tenant locally.
