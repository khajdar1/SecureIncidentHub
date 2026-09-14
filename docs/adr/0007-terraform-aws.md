# ADR 0007: Terraform and AWS as the first deployment target

Status: Accepted · 2026-09-14

## Context

Infrastructure should be reproducible while application design stays portable. Restricted AWS Free-plan services cannot be assumed available.

## Decision

Use Terraform with a resource-free lab root, AWS provider, `us-east-1`, documented local profile `new-profile-name`, provider lock and no remote backend. Plan Fargate/ECR, RDS, S3/SQS, ALB/CloudFront and monitoring incrementally. CI only validates. Future manual deployment uses temporary `aws login` credentials and STS preflight.

## Alternatives considered

Manual console changes are hard to reproduce. Cloud-specific IaC teaches fewer portable workflows. Kubernetes and multi-cloud add unjustified complexity. Terraform BSL licensing is recorded in the version inventory.

## Consequences

No resource/data source/backend/deploy script/IAM OIDC provider/IAM user/key is created in Phase 0. Ignore local state/plans. Add modules only alongside actual resources.

## Security implications

Never embed credentials in Terraform inputs/state. Future review must identify account, region, exposure, least privilege and protected state. CI has no AWS authentication.

## Operational implications

Budget/cleanup controls precede paid services. Restricted-account denial must fail clearly, with no Fargate-to-EC2 substitution. Separate lab and production-reference designs.
