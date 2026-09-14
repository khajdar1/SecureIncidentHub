# Security policy

Phase 0 is a development foundation, not a hosted service. Use no real incident evidence, personal data, production credentials or malware in the repository or its local services. No formal ASVS compliance or certification is claimed.

Report suspected vulnerabilities privately to the maintainer using GitHub private vulnerability reporting if enabled. Otherwise contact the maintainer privately to establish a channel before sharing details. Never include secrets or private evidence in public issues. This project does not promise a response SLA.

## Invariants

- Tenant data requires authenticated, current application membership and explicit organization-scoped access. Provider claims never grant an application role by themselves.
- Authentication is deny-by-default. Only the three documented diagnostic routes allow anonymous requests. Disabled OIDC is not an authentication bypass.
- Tokens, cookies, secrets, presigned URLs, evidence content and unnecessary personal data must not enter logs, traces, Git or CI artifacts.
- Domain/Application remain independent of web frameworks and external providers.
- CI has no AWS credentials/deployment and never runs privileged pull-request code.
- Local services bind to loopback; local env files and Terraform state are untracked.

Review tenant access, identity validation, integrity, sensitive output and supply-chain changes as security-relevant. Unimplemented controls are tracked in the [roadmap](docs/roadmap.md). The [threat model](security/threat-model/README.md) separates present and planned boundaries; [security testing](security/testing.md) describes verification.
