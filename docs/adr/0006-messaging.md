# ADR 0006: Messaging boundary with RabbitMQ locally and SQS on AWS

Status: Accepted for future implementation · 2026-09-14

## Context

Evidence inspection and notifications benefit from asynchronous work; basic incident validation does not. RabbitMQ/SQS differ in acknowledgement, routing and retries.

## Decision

Introduce an application messaging contract with the first real asynchronous use case. RabbitMQ is available locally; SQS is the AWS target. Require durable at-least-once delivery, correlation/causation IDs, idempotency, bounded retry and dead-letter handling. Use an outbox when a transaction and publication must agree.

## Alternatives considered

Synchronous processing delays requests and couples recovery to clients. Provider types leak coupling. In-memory queues cannot demonstrate durability.

## Consequences

Phase 0 has an idle worker and broker, without messaging SDKs, invented message types, publisher/consumer or outbox. Never claim exactly-once delivery.

## Security implications

Messages carry minimal opaque IDs, not tokens/evidence. Resolve tenant ownership from authoritative state; do not retry permanent validation failures.

## Operational implications

Define retry budgets, dead-letter ownership, replay controls and processing metrics with the first consumer. Both adapters need semantic contract tests.
