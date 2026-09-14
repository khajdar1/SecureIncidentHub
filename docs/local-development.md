# Local development

Install .NET SDK from `global.json`, Node from `.nvmrc` with npm 11, Git, Docker Desktop with Linux containers and Compose, and Terraform 1.15.x. Use Gitleaks/Trivy versions in [the version inventory](versions.md) for security checks. AWS CLI is optional in Phase 0. ZITADEL credentials, database/broker connectivity, storage and a telemetry backend are not required to build or test.

From the repository root:

```powershell
dotnet restore SecureIncidentHub.slnx --locked-mode
dotnet format SecureIncidentHub.slnx --no-restore --verify-no-changes
dotnet build SecureIncidentHub.slnx --no-restore -c Release
dotnet test SecureIncidentHub.slnx --no-build --no-restore -c Release
npm.cmd --prefix src/frontend ci --ignore-scripts
npm.cmd --prefix src/frontend run format:check
npm.cmd --prefix src/frontend run build
npm.cmd --prefix src/frontend test
```

On Linux/macOS use `npm` instead of `npm.cmd`. `scripts/verify.ps1` performs these checks plus Compose/Terraform/PowerShell validation; `-IncludeSecurity` adds scanners. Review and stage intended changes before its staged-secret check, which does not scan untracked files. Restore intentionally fails if lockfiles no longer match manifests. Use `dotnet format` / `npm.cmd --prefix src/frontend run format` to apply formatting, then review the diff.

Run the hosts in separate terminals:

```powershell
dotnet run --project src/backend/SecureIncidentHub.Api --no-launch-profile --urls http://127.0.0.1:5080
dotnet run --project src/backend/SecureIncidentHub.Worker --no-launch-profile
npm.cmd --prefix src/frontend start
```

Open `http://127.0.0.1:4200`. Request `/health/live`, `/health/ready`, or `/examples/problem` at port 5080. The worker stays alive until Ctrl+C, logging start/stop without polling an imaginary queue. API listener binding comes from the command; `AllowedHosts` checks the Host header and is not an interface-binding control. Use loopback HTTP only with synthetic data. Deployment TLS/proxy headers/CORS require later explicit configuration.

## Optional local data services

```powershell
./scripts/new-local-env.ps1
docker compose --env-file infra/compose/.env -f infra/compose/compose.yaml config --quiet
docker compose --env-file infra/compose/.env -f infra/compose/compose.yaml up -d --wait --wait-timeout 120
docker compose --env-file infra/compose/.env -f infra/compose/compose.yaml ps
```

The helper creates ignored random development credentials without printing them and refuses to overwrite an existing `.env`. Alternatively copy `.env.example` to `.env` and replace every placeholder with unique local values. Never start with example placeholders. Shell environment variables take precedence over Compose env-file values, so clear stale `POSTGRES_*` / `RABBITMQ_*` variables if configuration is unexpected. Use `config --quiet`; plain `config` prints resolved credentials.

PostgreSQL binds 127.0.0.1:5432; RabbitMQ AMQP/management bind 127.0.0.1:5672/15672. They share the Compose network, with separate named volumes and no TLS. Loopback protects against ordinary remote access, not hostile local users/Docker administrators. Credentials initialize new volumes only: changing `.env` does not rotate credentials already stored in an existing volume. Synthetic data only. MinIO is omitted; local object storage awaits owner-reviewed supported candidates.

Stop with `docker compose --env-file infra/compose/.env -f infra/compose/compose.yaml down`; it retains data. See [cleanup/restore guidance](runbooks/README.md) before deleting volumes. If Docker reports a missing engine named pipe, start Docker Desktop and wait for the Linux engine. Do not interpret `config --quiet` as a runtime health check.

## Terraform and future AWS identity

```powershell
terraform -chdir=infra/terraform/environments/lab fmt -check
terraform -chdir=infra/terraform/environments/lab init -backend=false -input=false -lockfile=readonly
terraform -chdir=infra/terraform/environments/lab validate
./scripts/test-foundation.ps1
```

These checks never invoke AWS. The foundation test mocks `aws` in its own process to verify profile/region precedence, readback and failure handling; no credentials are consulted.

For a **future, separately authorized local deployment preparation**, the documented profile example is `new-profile-name`:

```powershell
aws login --profile new-profile-name
./scripts/aws-preflight.ps1 -AwsProfile new-profile-name
# Or set AWS_PROFILE and invoke the preflight without -AwsProfile.
```

The script checks CLI availability, runs only STS GetCallerIdentity and displays selected account/ARN/profile/region. `-AwsRegion` / `AWS_REGION` defaults to `us-east-1`; other regions fail. Missing profile or authentication fails clearly. Profile selection follows the user's Phase 0 instruction, superseding the earlier fixed-profile wording in AGENTS.md. Identity success does not verify Fargate/RDS/etc. availability. No deploy script or Terraform apply belongs in Phase 0.

Windows may block local scripts under its execution policy. After reviewing repository scripts, a process-only invocation such as `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test-foundation.ps1` runs that check without changing machine/user policy. Substitute the desired reviewed script filename. This is how scripts were verified on the initial machine.
