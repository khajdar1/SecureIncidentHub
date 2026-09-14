# Architecture decisions

Accepted design does not imply future adapters/features are implemented. Supersede accepted history with a new ADR. Identity provider, authorization ownership and browser token handling are separate decisions because each can change independently.

- [Modular monolith](0001-modular-monolith.md)
- [PostgreSQL as the system of record](0002-postgresql.md)
- [ZITADEL Cloud through standard OIDC](0003-zitadel-oidc.md)
- [Application-owned authorization and tenant isolation](0004-application-authorization.md)
- [Provider-neutral object storage; local adapter deferred](0005-object-storage.md)
- [Messaging boundary with RabbitMQ locally and SQS on AWS](0006-messaging.md)
- [Terraform and AWS as the first deployment target](0007-terraform-aws.md)
- [OpenTelemetry as the observability standard](0008-opentelemetry.md)
- [SPA PKCE initially; BFF retained as a future option](0009-spa-pkce.md)
