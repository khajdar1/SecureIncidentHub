# ADR 0005: Provider-neutral object storage; local adapter deferred

Status: Accepted for port and S3 target; local implementation Deferred · 2026-09-14

## Context

Evidence bytes need private object storage and separate metadata/authorization. MinIO Community is archived and unmaintained according to its [upstream repository](https://github.com/minio/minio). The owner explicitly rejected adding it or another unmaintained dependency.

## Decision

Retain application-owned `IObjectStorage` with provider-neutral object references and bounded upload/download grants. Prepare configuration and document S3 as the production target. Install no storage SDK or local container. Hosts/tests require no object storage.

## Alternatives considered

A pinned archived MinIO image was rejected. Before selecting a supported local alternative, present maintenance, licensing, compatibility, resource use and operational tradeoffs to the owner. No replacement is selected.

## Consequences

Contracts describe requirements, not a fake backend. Presigning/content verification and provider contract tests remain unimplemented.

## Security implications

Authorize ownership before storage calls; generate opaque keys server-side; enforce size/type/expiry and private storage. Adapters must reject restrictions they cannot enforce, and grants must never be logged.

## Operational implications

Compose has only PostgreSQL/RabbitMQ. Local adapter selection is a gate before evidence work. S3 cost/availability remain unverified; no bucket exists.
