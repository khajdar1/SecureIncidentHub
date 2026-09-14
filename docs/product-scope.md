# Product scope

Reporters submit incidents and evidence within their organization. Security Analysts classify severity/category, assign and investigate incidents, and control explicit resolution transitions. Organization Administrators manage application membership and roles within one organization. ZITADEL Cloud owns authentication, MFA and identity lifecycle.

The first usable release needs incident reporting/review, private evidence uploads, authorized downloads, idempotent asynchronous processing, an append-oriented audit trail, notifications and operational monitoring. PostgreSQL owns memberships, permissions, incident state and evidence metadata; private object storage owns evidence bytes.

Phase 0 delivers only repository standards, executable hosts, configuration boundaries, diagnostics, tests, architecture/security decisions and CI. A passing health check does not mean that the product works.

Non-goals: a custom identity provider/login page, cross-organization administration, real malware, microservices, Kubernetes, event sourcing, CQRS frameworks, service meshes, multiple cloud deployments, a BFF implementation, production readiness or deployment. No object-storage replacement is selected before supported candidates and tradeoffs are reviewed with the owner.
