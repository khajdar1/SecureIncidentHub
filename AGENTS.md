# Secure Incident Hub — Repository Instructions

## 1. Mission

Build a portfolio-quality, security-first, cloud-native incident and evidence management platform.

The project must demonstrate transferable industry knowledge rather than knowledge tied exclusively to AWS. AWS is the first production deployment target, but application architecture, protocols, security controls, observability, testing and documentation must remain portable wherever reasonably possible.

This repository is also a learning project. Every important implementation must make the underlying engineering concept understandable to the repository owner.

The system allows employees to report security incidents and upload evidence, while security analysts classify, investigate and resolve incidents. Organizations are isolated from one another.

## 2. Decision priorities

When requirements conflict, use this order:

1. Security and correctness
2. Data isolation and integrity
3. Clarity and maintainability
4. Testability and observability
5. Vendor-neutral design
6. Cost efficiency
7. Delivery speed

Do not introduce complexity only to make the project appear more advanced.

Prefer the simplest architecture that meets the actual requirements and leaves clear extension points.

Never hide failing behavior, bypass quality checks or report success without verification.

## 3. Product scope

Initial roles:

* Reporter
* Security Analyst
* Organization Administrator

Initial capabilities:

* Sign in through an external OIDC provider
* Create and review security incidents
* Assign category, severity and status
* Upload incident evidence securely
* Process evidence asynchronously
* Maintain an append-oriented audit trail
* Notify relevant users about changes
* Enforce organization-level tenant isolation
* Monitor system health and suspicious activity

An authenticated user from one organization must never be able to access another organization’s incidents, evidence, audit records or administrative functions.

## 4. Approved architecture

Use a modular monolith for the main application.

Primary components:

* Angular frontend
* ASP.NET Core API
* ASP.NET Core background worker
* PostgreSQL
* Object storage
* Message broker or managed queue
* External OpenID Connect identity provider
* OpenTelemetry-based observability
* Terraform infrastructure
* GitHub Actions CI/CD

Local implementations:

* PostgreSQL
* MinIO for S3-compatible object storage
* RabbitMQ for messaging
* Docker Compose
* ZITADEL Cloud remains external and is not emulated locally

Initial AWS implementations:

* Amazon ECS with Fargate
* Amazon RDS for PostgreSQL
* Amazon S3
* Amazon SQS
* Amazon ECR
* Application Load Balancer
* Amazon CloudFront
* AWS IAM and STS
* AWS KMS
* AWS Secrets Manager
* Amazon CloudWatch
* AWS CloudTrail
* Optional AWS WAF, GuardDuty and Config

Do not introduce Kubernetes, microservices, event sourcing, CQRS frameworks, service meshes or multiple cloud deployments without a documented requirement and accepted Architecture Decision Record.

## 5. Module boundaries

Initial application modules:

* Identity
* Organizations
* Incidents
* Evidence
* Audit
* Notifications

Maintain explicit dependencies between modules.

Domain and application layers must not depend directly on AWS SDKs, MinIO, RabbitMQ, Entity Framework implementation details or web frameworks.

Use infrastructure adapters for external systems such as:

* Object storage
* Messaging
* Identity
* Email or notifications
* Time
* Cloud-specific services

Examples of appropriate boundaries:

* `IObjectStorage`
* `IMessagePublisher`
* `ICurrentIdentity`
* `IAuditWriter`
* `IClock`

Do not create interfaces or abstractions that have no meaningful alternative, testability benefit or architectural purpose.

Do not create a generic repository abstraction over Entity Framework solely for pattern compliance. Keep persistence access explicit and understandable.

Controllers must remain thin. Business rules belong in application or domain services, not controllers, UI components or infrastructure adapters.

## 6. Identity and authorization

Use ZITADEL Cloud as the initial OpenID Connect provider.

For the Angular SPA, use Authorization Code Flow with PKCE.

Never place a client secret in Angular or any browser-delivered code.

The API must validate:

* Token signature
* Issuer
* Audience
* Expiration
* Not-before time
* Required scopes
* Authentication scheme

Identify external users using the immutable combination of:

* `iss`
* `sub`

Do not use email as a permanent user identifier.

ZITADEL is responsible for authentication, token issuance, MFA and identity lifecycle.

The application database is the source of truth for:

* Organizations
* Membership
* Application roles
* Permissions
* Resource ownership
* Tenant isolation

Never trust an `organizationId`, `userId`, role or ownership claim received from request data without server-side authorization.

Authorization is deny-by-default.

Every endpoint accessing tenant-owned data must have an automated cross-tenant access test.

Start with SPA PKCE and access tokens held in memory. Do not store access or refresh tokens in `localStorage`.

A Backend-for-Frontend deployment may be introduced later through a separate ADR.

## 7. Security standards

Align the project with:

* OWASP Application Security Verification Standard 5.0, targeting Level 2 where applicable
* OWASP Top 10:2025 for risk awareness
* OWASP threat-modeling guidance
* NIST Secure Software Development Framework
* OpenSSF software supply-chain recommendations
* Principle of least privilege
* Secure-by-default and deny-by-default design
* Defense in depth
* Zero Trust principles where applicable
* WCAG 2.2 AA for relevant frontend behavior

Do not claim formal compliance or certification unless it has actually been independently verified.

Security requirements should reference applicable ASVS requirement identifiers where practical.

Never implement custom cryptographic algorithms.

Use established platform cryptography and identity libraries.

Never log:

* Passwords
* Access or refresh tokens
* Session cookies
* Secret values
* Private encryption material
* Complete sensitive evidence
* Unnecessary personal information

Sensitive values must be redacted from errors, logs, traces and CI output.

## 8. Threat modeling

Maintain the threat model under `security/threat-model/`.

The threat model must include:

* Protected assets
* Actors
* Entry points
* Data flows
* Trust boundaries
* Threats
* Existing controls
* Planned controls
* Verification method
* Residual risk
* Accepted risks

Use data-flow diagrams and STRIDE as the initial methodology.

Update the threat model whenever a change introduces:

* A new external integration
* A new trust boundary
* A new data store
* A new authentication flow
* A new privileged role
* A new public endpoint
* A new type of sensitive data
* A new background process

Threat-model changes must be committed with the implementation that caused them.

## 9. Multi-tenancy requirements

All tenant-owned records must carry an explicit organization identifier.

Tenant filtering must occur server-side.

Never rely on the frontend to enforce tenant boundaries.

Database queries must make tenant scope visible and reviewable.

Privileged cross-organization operations must be exceptional, explicitly authorized and audited.

Required security tests include:

* Same-tenant authorized access
* Same-tenant unauthorized-role access
* Cross-tenant read attempt
* Cross-tenant update attempt
* Cross-tenant evidence access
* Direct object reference manipulation
* Disabled or removed membership
* Changed role with an existing session

## 10. Evidence upload requirements

Evidence must be uploaded using time-limited presigned URLs where supported.

Validate:

* Maximum size
* Allowed type
* Actual content where feasible
* Filename handling
* Storage key generation
* Organization ownership
* Upload expiration
* Processing state

Do not trust the original filename as a storage path.

Generate storage identifiers server-side.

Evidence storage must be private by default.

Downloads must require authorization and use short-lived URLs or an authorized streaming endpoint.

Asynchronous evidence processing must be idempotent.

Record evidence hashes where appropriate for integrity and audit purposes.

Do not include real malicious software in the repository.

## 11. Messaging and background processing

Introduce messaging only for work that benefits from asynchronous execution.

When messaging is introduced:

* Use durable messages
* Include correlation and causation identifiers
* Make consumers idempotent
* Define retry behavior
* Define dead-letter handling
* Do not retry permanent validation failures
* Prevent sensitive data from being unnecessarily copied into messages
* Document delivery guarantees

Use a transactional outbox when a business transaction and message publication must remain consistent.

Do not describe a workflow as “exactly once” unless that property can actually be demonstrated.

## 12. API standards

Use explicit HTTP semantics and consistent response formats.

Use Problem Details for API errors.

Validate all input at system boundaries.

Return safe client messages while retaining diagnosable internal logs.

Do not expose stack traces or implementation details to clients.

Use pagination for potentially unbounded collections.

Use UTC internally and ISO 8601 timestamps at boundaries.

Make state transitions explicit and enforce them server-side.

Document the API with OpenAPI.

Breaking API changes require an ADR or an explicit versioning decision.

## 13. Database rules

Use PostgreSQL as the source of truth.

Schema changes must use reviewed migrations.

Migrations must be deterministic and suitable for automated deployment.

Do not silently delete or rewrite production-like data.

Add indexes based on demonstrated query patterns, constraints or measured behavior.

Use database constraints for invariants that must remain true regardless of application code.

Avoid storing secrets in the application database.

Audit records must not be casually editable through normal application workflows.

Seed data must be clearly marked as development-only.

## 14. Observability

Use OpenTelemetry APIs and conventions where practical.

Implement:

* Structured logs
* Correlation IDs
* Distributed traces
* Request duration metrics
* Error-rate metrics
* Background-processing metrics
* Liveness checks
* Readiness checks

Logs must be useful for investigation without exposing secrets or unnecessary personal data.

Do not swallow exceptions.

Unexpected exceptions must:

* Be logged once at the appropriate boundary
* Preserve correlation information
* Return a safe external response
* Trigger appropriate failure behavior

Define initial service-level indicators and objectives in documentation. Treat them as engineering targets, not fabricated guarantees.

## 15. Testing strategy

Use multiple test levels:

* Unit tests for domain rules and state transitions
* Integration tests for database, authorization and infrastructure adapters
* API tests for contracts and error handling
* Architecture tests for dependency boundaries where valuable
* End-to-end tests for critical user journeys
* Security-focused tests for tenant isolation and access control
* Terraform validation and infrastructure-policy checks

Prefer realistic integration tests over excessive mocking.

Use disposable test infrastructure or containers where practical.

Tests must be deterministic and independent.

Do not weaken or remove a legitimate test merely to make CI pass.

When fixing a defect, add a regression test whenever reasonably possible.

Coverage is a diagnostic signal, not a vanity target. Prioritize critical paths, authorization rules and failure modes.

## 16. Infrastructure as Code

Infrastructure must be reproducible through Terraform.

Maintain separate concepts for:

* Cost-conscious lab deployment
* Production-reference architecture

Avoid undocumented manual console changes.

Every manual emergency action must be documented and later represented in code where appropriate.

Terraform must use:

* Clear module boundaries
* Explicit inputs and outputs
* Stable naming conventions
* Required tags
* Least-privilege IAM
* Encryption in transit and at rest
* Protected state storage
* Controlled environment separation

Do not place plaintext secrets in Terraform code, variables, plans, outputs or state when a managed alternative exists.

Do not create paid cloud resources, run `terraform apply`, run `terraform destroy`, register domains or modify remote infrastructure without explicit user authorization.

Before an authorized deployment:

* Show the planned resources
* Identify public exposure
* Estimate meaningful recurring costs
* Confirm the AWS account and region
* Confirm cleanup steps

Budget and cost controls must be created before optional or expensive services.

Avoid leaving NAT Gateways, load balancers, databases, WAF rules or security services active unnecessarily in learning environments.

## 17. CI/CD and supply-chain security

CI must progressively enforce:

* Formatting
* Linting
* Compilation
* Unit tests
* Integration tests where practical
* Static analysis
* Secret scanning
* Dependency scanning
* Container scanning
* Infrastructure scanning
* Terraform formatting and validation
* SBOM generation
* OpenSSF Scorecard checks when suitable

GitHub Actions workflows must use minimum permissions.

Use OIDC and short-lived AWS STS credentials. Never use permanent AWS access keys for CI deployment.

Pin third-party GitHub Actions to immutable commit SHAs and include a readable version comment.

Do not execute untrusted pull-request code with privileged credentials.

Build artifacts and container images should be traceable to their source commit.

Do not report a security scan as passing unless it was actually executed successfully.

## 18. Architecture documentation

Maintain architecture documentation under `docs/architecture/`.

Use C4 diagrams where useful:

* System Context
* Container
* Component only when it adds real value
* Deployment view

Also maintain a security-focused data-flow diagram with trust boundaries.

Diagrams must include:

* Element names
* Responsibilities
* Technologies
* Directional relationships
* Relevant protocols
* Trust boundaries where applicable

Do not create diagrams merely to increase documentation volume.

## 19. Architecture Decision Records

Store ADRs under `docs/adr/`.

Create or update an ADR when a decision:

* Changes a system boundary
* Introduces a major dependency
* Changes data ownership
* Changes authentication or authorization
* Introduces a cloud-specific coupling
* Has an important security implication
* Is difficult or expensive to reverse
* Rejects a plausible alternative

Each ADR must include:

* Context
* Decision
* Alternatives considered
* Consequences
* Security implications
* Operational implications
* Status

Do not rewrite accepted ADR history to pretend the original decision never existed. Supersede it with a new ADR.

## 20. Documentation requirements

Maintain:

* `README.md`
* Architecture documentation
* ADRs
* Threat model
* Local-development instructions
* Deployment runbooks
* Rollback runbook
* Backup and restore runbook
* Incident-response runbook
* Cost and cleanup guidance
* Security-testing documentation
* Learning log

Documentation must describe why important decisions were made, not only what files and tools exist.

Update relevant documentation in the same commit as behavior-changing code.

Do not generate large amounts of generic filler documentation.

## 21. Learning requirement

The repository owner must be able to explain the system without depending on the AI that implemented it.

After every meaningful task, provide a concise section titled `What you should understand` covering:

* The vendor-neutral concept
* How it is implemented here
* Why the decision was made
* The most important alternative
* How the implementation was verified

Maintain `docs/learning-log.md` with concise entries for major concepts and decisions.

Do not obscure important behavior behind generated abstractions without explanation.

## 22. Git workflow

Before changing files:

1. Read this file.
2. Read the README, relevant ADRs and threat model.
3. Inspect `git status`.
4. Preserve all existing user changes.
5. Identify the exact task and acceptance criteria.
6. Produce a short plan for non-trivial work.

Never:

* Force push
* Delete branches without authorization
* Run destructive resets
* Discard user changes
* Rewrite published history
* Amend an existing commit without authorization
* Commit secrets
* Push to a remote unless explicitly requested

Use feature branches when appropriate:

* `feat/<short-name>`
* `fix/<short-name>`
* `security/<short-name>`
* `docs/<short-name>`
* `chore/<short-name>`

## 23. Commit policy

Use Conventional Commits 1.0.0.

Allowed common types:

* `feat`
* `fix`
* `security`
* `refactor`
* `test`
* `docs`
* `build`
* `ci`
* `chore`
* `perf`

Format:

`type(optional-scope): concise imperative description`

Examples:

* `feat(incidents): add incident severity transitions`
* `security(auth): reject cross-tenant evidence access`
* `fix(worker): make evidence processing idempotent`
* `test(authorization): cover removed organization membership`
* `docs(architecture): record object storage decision`
* `ci(security): add secret and container scanning`

Commit one coherent, reviewable change at a time.

A commit should represent a meaningful unit of behavior or project configuration, not:

* Every edited file
* Every few minutes of work
* Unrelated changes grouped together
* An unverified work-in-progress snapshot

Avoid vague messages such as:

* `update`
* `changes`
* `fix stuff`
* `wip`
* `AI changes`
* `misc`

Before committing:

1. Review the complete diff.
2. Ensure no unrelated user changes are included.
3. Run relevant formatting, builds and tests.
4. Run relevant security checks.
5. Confirm documentation remains accurate.
6. Check for secrets and sensitive data.

Do not commit if the repository is knowingly broken unless the user explicitly requests a checkpoint commit. Clearly mark and explain any authorized checkpoint.

Do not add “generated by AI” or similar wording to commit messages.

## 24. Dependency management

Before adding a dependency:

* Confirm it solves a real requirement
* Prefer maintained and widely used libraries
* Review its official documentation
* Check its license
* Check known security concerns
* Prefer supported stable releases
* Avoid duplicating existing capabilities
* Record major architectural dependencies in an ADR

Do not silently upgrade major versions.

Do not introduce a library only to avoid writing a small and clear amount of application code.

Dependabot or an equivalent update mechanism should be configured with controlled update behavior.

## 25. AI working behavior

Always inspect before editing.

Do not assume repository structure, installed tools, cloud credentials or successful commands.

For non-trivial work:

1. State the intended outcome.
2. Inspect relevant files and current Git state.
3. Create a short implementation plan.
4. Implement the smallest coherent slice.
5. Verify it.
6. Review the diff.
7. Update documentation.
8. Commit the verified slice with a meaningful message.
9. Report the result and remaining work.

Ask for clarification only when a missing decision materially changes architecture, security, cost, data handling or user-visible behavior.

Do not stop for minor decisions that can be handled through a safe, documented assumption.

Do not:

* Fabricate command output
* Claim unexecuted tests passed
* Add fake implementations
* Leave hidden placeholders
* Add broad catch blocks that suppress errors
* Disable analyzers to silence legitimate findings
* Comment out failing tests
* Add unrelated refactors
* Replace working code wholesale without justification
* Create remote resources without authorization
* Push commits without authorization

TODO comments must reference an explicit tracked backlog item or clearly explain why the work cannot yet be completed.

## 26. Task completion report

At the end of every task, report:

* Outcome
* Main files changed
* Architectural or security impact
* Commands and tests executed
* Results of those commands
* Commit hash and message, if committed
* Known limitations
* Recommended next step
* What you should understand

Be explicit about anything that could not be verified.

## 27. Definition of Done

A task is complete only when:

* Acceptance criteria are met
* Security and tenant isolation were considered
* Code is formatted and builds successfully
* Relevant tests pass
* Failure paths were considered
* No secrets or sensitive data were introduced
* Observability is adequate for the new behavior
* Documentation and diagrams remain accurate
* Threat model and ADRs were updated when required
* The final diff was reviewed
* The work is committed as a coherent change when committing was requested
* Remaining limitations are disclosed

A passing build alone does not mean the task is complete.

## AWS account constraints

- The AWS account uses the restricted AWS Free plan.
- Use `us-east-1` for every regional AWS resource.
- Do not require or attempt to activate AWS advanced features.
- Do not create an IAM OIDC identity provider.
- Do not store long-lived AWS access keys in GitHub Secrets or repository files.
- GitHub Actions must initially perform CI only: build, test, lint, dependency scanning,
  SAST, Docker image validation, and Terraform validation.
- AWS deployment is executed manually from the developer's local machine using
  temporary credentials obtained through:

  aws login --profile new-profile-name

- Before any AWS operation, verify the identity with:

  aws sts get-caller-identity --profile new-profile-name

- Terraform and deployment scripts must use the `new-profile-name` AWS profile
  and the `us-east-1` region.
- Provide PowerShell-compatible deployment scripts for Windows.
- Infrastructure code may be created before deployment, but never assume that
  an AWS service is available. Perform a preflight check and fail clearly when
  the restricted account blocks a required operation.
- Never silently replace ECS Fargate with EC2 or another architecture merely
  because an AWS feature is unavailable.