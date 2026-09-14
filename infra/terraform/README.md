# Terraform foundation

`environments/lab` is the only root. It contains an AWS provider and non-secret defaults, no resources/data sources/backend. Run from that directory:

```powershell
terraform fmt -check
terraform init -backend=false -input=false -lockfile=readonly
terraform validate
```

For a deliberate provider update, omit `-lockfile=readonly`, review the change, run `terraform providers lock -platform=linux_amd64 -platform=windows_amd64`, and commit the reviewed lock. Initialization downloads provider binaries but does not invoke AWS identity APIs. Never use `plan` or `apply` as a Phase 0 check.

Future modules and a distinct production-reference root will be introduced alongside actual resources. Protected remote state, budget controls, Free-plan service preflight and explicit deployment authorization must precede deployment. There is no `scripts/deploy.ps1`.
