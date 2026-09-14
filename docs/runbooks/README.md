# Phase 0 operational runbooks

There is no deployed service or application data schema. These procedures apply only to the local foundation; later phases must replace the documented gaps before deployment.

## Deployment and cost

No Phase 0 deployment is authorized. Terraform has no resources/backend. Future deployment starts with `aws login --profile new-profile-name`, then `scripts/aws-preflight.ps1`; it only identifies the account and cannot prove Free-plan service availability. Before any resource change, review actual plan, region, exposure, recurring costs, budget controls and cleanup steps. No deploy script exists yet.

## Rollback

For a failed local change, preserve uncommitted work and create a reviewed revert of the offending commit; rerun checks. Do not reset/discard user work. No schema rollback procedure is claimed. Future migrations must document forward-repair and restore implications.

## Backup and restore

Local named volumes preserve PostgreSQL/RabbitMQ across `docker compose down`; they are not backups. Phase 0 stores no real evidence or application records. Phase 1 must demonstrate a PostgreSQL dump/restore into a disposable separate instance, verify ownership/constraints and record RPO/RTO targets. Evidence backup/versioning and retention are deferred with the adapter.

## Incident response

If a local secret is exposed, stop the affected local service, restrict access, rotate the credential and investigate sanitized logs. For Git exposure, revoke first and coordinate remediation with the owner; never silently rewrite published history. Record UTC timeline, affected component, commit/version and correlation IDs without copying secrets. Use the private reporting guidance in `SECURITY.md`.

## Local cleanup

From the repository root, `docker compose --env-file infra/compose/.env -f infra/compose/compose.yaml down` removes containers/network and retains named volumes. Add `--volumes` only after explicitly deciding the data is disposable; it deletes local database and broker data. No automatic script deletes volumes or cloud resources. Keep local secrets outside Git and use synthetic data only.
