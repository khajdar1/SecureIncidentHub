# Security data-flow diagram

Solid arrows are current local flows; dashed arrows are future or explicitly enabled integrations. Application hosts have no database, messaging or storage client in Phase 0. Module/project boundaries are not security sandboxes.

```mermaid
flowchart LR
  subgraph browser["TB1 · Untrusted browser/client"]
    spa["Angular shell
Future memory-only access token"]
    caller["HTTP caller
Controls request data and headers"]
  end
  subgraph app["TB2 · Application process authority"]
    api["ASP.NET Core API
JWT fallback and safe diagnostics"]
    worker[".NET worker
Lifecycle only; consumers planned"]
  end
  subgraph local["TB3 · Developer host and Docker authority"]
    env["Ignored infra/compose/.env
Synthetic bootstrap credentials"]
    pg[("PostgreSQL
127.0.0.1:5432; named volume")]
    mq["RabbitMQ
127.0.0.1:5672/15672; named volume"]
  end
  subgraph provider["TB4 · External identity authority"]
    idp["ZITADEL Cloud
Authentication and signing keys"]
  end
  subgraph storage["TB5 · Future private evidence authority"]
    s3[("Amazon S3 planned
Local provider deferred")]
  end
  subgraph ops["TB6 · Build and operator authority"]
    ci["GitHub Actions
Untrusted PR builds; read-only token"]
    source["Source, package locks and provider lock"]
    logs["JSON console logs
Safe type, trace/status metadata"]
    otlp["Trusted OTLP destination
Not provisioned"]
  end
  caller -->|HTTP loopback diagnostic request/response| api
  env -->|Compose environment injection| pg
  env -->|Compose environment injection| mq
  api -->|Structured logs| logs
  worker -->|Lifecycle logs| logs
  source -->|Checkout and dependency restore| ci
  spa -. "HTTPS OIDC code + PKCE" .-> idp
  spa -. "HTTPS scoped bearer requests" .-> api
  api -. "HTTPS discovery/JWKS if enabled" .-> idp
  api -. "Scoped SQL; TLS deployed" .-> pg
  api -. "Outbox then AMQP/SQS" .-> mq
  mq -. "Durable at-least-once messages" .-> worker
  api -. "Authorized short-lived grants" .-> s3
  spa -. "HTTPS grant; private evidence" .-> s3
  worker -. "Authorized evidence read" .-> s3
  api -. "Opt-in OTLP" .-> otlp
  worker -. "Opt-in OTLP" .-> otlp
```

Future AWS control-plane authority is separate: a developer selects temporary local credentials, verifies STS identity, then separately reviews/authorizes resource changes. Phase 0 CI has no AWS authentication or deployment path. The two local services share a Docker network; no inter-service isolation or TLS is claimed.
