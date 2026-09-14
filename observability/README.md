# Observability baseline

API and worker use structured JSON console logs. API responses include a generated or inherited W3C trace ID in `X-Correlation-ID` and Problem Details `traceId`; a client-supplied correlation header is never trusted. The exception boundary records safe exception type and trace ID once, without exception message/data/stack contents that might contain secrets. ASP.NET request info logs are restricted because they can include URLs. JWT internal diagnostics are suppressed; HTTP status metrics remain observable.

OpenTelemetry registers ASP.NET server spans and `http.server.request.duration` metrics (status dimensions allow error-rate calculation). URL/query tags are removed before span export; headers, bodies and exception events are not captured. The worker emits `hub.worker.running` (1 while started, 0 on shutdown). It has no processing throughput metric because there is no consumer yet.

No export occurs by default. Set `OTEL_EXPORTER_OTLP_ENDPOINT` only for a trusted collector; see `.env.example`. Exporter defaults/protocol follow the standard .NET OTLP exporter. No collector/container/backend is required or deployed. Collector availability, exported signal shape and alerting remain to be tested before deployment. Never enable payload/header capture or add unbounded IDs as metric dimensions.

## Initial engineering targets, not service guarantees

| Future SLI | Initial target | Verification before release |
| --- | --- | --- |
| Valid API requests returning non-5xx | 99.5% over 30 days, excluding health probes and deliberate error example | Dashboard from route/status metrics plus injected failure |
| API latency excluding uploads | p95 under 500 ms at documented lab load | Repeatable load profile and duration histogram |
| Accepted evidence completing processing | 99% within 5 minutes over 7 days | Consumer completion timestamps, retry/dead-letter exercise |

Health probes are `GET /health/live` (process responds) and `/health/ready` (host started and not stopping). Add dependency checks only when the API actually needs those systems; do not imply unused services are healthy. Worker readiness is not exposed over HTTP. Future consumer health/readiness and queue lag belong to the messaging slice.
