# Security verification

Phase 0 includes xUnit tests for exact identity keys, configuration rejection, host readiness, worker lifecycle and project dependencies. In-process API integration tests validate three production routes, safe Problem Details, correlation behavior, enabled/disabled identity, exact scope and signed JWT failure cases. Ephemeral RSA keys and static metadata avoid ZITADEL/network dependencies. No database or storage integration is claimed.

Run from the root after staging only intended changes:

```powershell
gitleaks git . --log-opts=--all --config security/gitleaks.toml --redact
gitleaks git . --pre-commit --staged --config security/gitleaks.toml --redact
npm.cmd --prefix src/frontend audit --audit-level=high
dotnet list SecureIncidentHub.slnx package --vulnerable --include-transitive
trivy filesystem --config security/trivy.yaml --scanners vuln,misconfig .
./scripts/test-foundation.ps1
```

NuGet restore also audits direct/transitive packages with warnings treated as errors. Trivy gates HIGH/CRITICAL findings, including development dependencies and unfixed vulnerabilities; no CVE ignore list exists. Gitleaks scans history and staged content, while the foundation check rejects tracked environment/state/build/key files. A directory scan can intentionally detect ignored local `.env` credentials; do not suppress that file from Git scanning. Never publish raw logs containing credentials. Keep machine-generated reports under ignored `artifacts/`; commit only reviewed summaries under `security/reports/`.

The baseline SAST controls are .NET recommended analyzers, warnings-as-errors and strict TypeScript/Angular template compilation. These do not equal a full security audit. Compose syntax validation does not scan or prove container health. CI additionally starts disposable PostgreSQL/RabbitMQ and waits for configured health checks; actual image vulnerability scanning, SBOM publication, CodeQL and Scorecard are future gates before deployable artifacts exist.

Before the first tenant endpoint, add real PostgreSQL tests for same-tenant allowed/denied roles, cross-tenant read/update, direct object reference manipulation, disabled/removed membership and changed roles with existing sessions. Evidence adds cross-tenant grants/downloads, size/type/content checks, server-generated keys, expiry, integrity and processing-state tests. Before messaging, verify outbox consistency, duplicate delivery, retry budgets and dead-letter/replay authorization.

Target OWASP ASVS 5.0 Level 2 where applicable, without claiming certification. Map exact requirement IDs when implementing each control; do not reuse older ASVS numbering unverified. The STRIDE model provides verification priorities, not confirmed vulnerabilities.
