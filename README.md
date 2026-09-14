# SecureIncidentHub

An organization-isolated security incident and evidence management platform, built as a learning and portfolio project. Employees report incidents; security analysts investigate; organization administrators manage membership. It is not ready for sensitive data or production use.

Phase 0 establishes Angular, ASP.NET Core API/worker hosts, tests, local PostgreSQL/RabbitMQ, architecture decisions and credential-free CI. There are no incident workflows, database schema, live login, evidence processing or deployed AWS resources yet. Local object storage is deferred because MinIO Community is unmaintained; no replacement has been selected.

## Start here

- [Local development and verification](docs/local-development.md)
- [Scope](docs/product-scope.md), [roadmap](docs/roadmap.md), [Definition of Done](docs/definition-of-done.md)
- [Architecture and C4 diagrams](docs/architecture/README.md), [ADRs](docs/adr/README.md)
- [Identity configuration](docs/architecture/identity.md), [threat model](security/threat-model/README.md)
- [Version choices](docs/versions.md), [security testing](security/testing.md), [learning log](docs/learning-log.md)
- [Runbooks](docs/runbooks/README.md), [security policy](SECURITY.md)
- [Phase 0 verification results and limitations](security/reports/phase-0.md)

## Repository map

```text
src/frontend/                     Angular SPA and component tests
src/backend/                      API, Worker, Domain, Application, Infrastructure
tests/                            .NET unit and API integration tests
infra/compose/                    PostgreSQL and RabbitMQ; development only
infra/terraform/environments/lab/  AWS provider configuration; no resources/backend
observability/                    Signal configuration and engineering targets
docs/architecture/                C4 views, OpenAPI and identity boundaries
docs/adr/                         Architecture decisions and deferred choices
security/                         Threat model, testing guidance and reports
scripts/                          PowerShell verification and AWS preflight
.github/                          CI-only quality gates and dependency updates
```

The API exposes `GET /health/live`, `GET /health/ready`, and `GET /examples/problem`. Readiness initially measures host startup only. The public example returns fixed 400 Problem Details. The API contract is a static OpenAPI file; no documentation endpoint is exposed.

AWS is the planned first target, using `us-east-1` and temporary local credentials. Phase 0 never deploys, authenticates CI to AWS, or creates paid resources. Follow [AGENTS.md](AGENTS.md) for engineering requirements.
