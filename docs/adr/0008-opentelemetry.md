# ADR 0008: OpenTelemetry as the observability standard

Status: Accepted · 2026-09-14

## Context

HTTP failures and asynchronous work need correlation without coupling application code to a telemetry vendor.

## Decision

Use .NET Activity/Meter with OpenTelemetry infrastructure integrations. Establish API request spans, duration/error dimensions, structured console logs and worker lifecycle metrics. OTLP export is opt-in. Record data restrictions and SLI/SLO targets under `observability/`.

## Alternatives considered

Vendor SDKs spread coupling. Logs alone cannot measure distributions or correlate asynchronous causality reliably. A full local telemetry stack is unnecessary for Phase 0.

## Consequences

No collector/backend is required for tests. End-to-end export, dashboards and alerts are deferred. Do not collect query strings, headers or bodies.

## Security implications

Exclude tokens, evidence and presigned URLs. Telemetry destinations are a separate trusted-operator boundary. Correlation IDs never grant authorization.

## Operational implications

Set retention/cardinality/sampling budgets before deployment. Host readiness does not claim database, queue or identity-provider health.
