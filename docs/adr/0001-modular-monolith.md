# ADR 0001: Modular monolith

Status: Accepted · 2026-09-14

## Context

One owner is learning incident management. Membership, incident state, evidence metadata and audit intent need clear ownership and consistent transactions.

## Decision

Use one modular application with API and worker hosts. Domain, Application and Infrastructure assemblies enforce dependency direction. Introduce Identity, Organizations, Incidents, Evidence, Audit and Notifications namespaces as slices arrive. Use explicit application services and persistence; no mediator/CQRS framework or generic repository.

## Alternatives considered

Microservices add distributed authorization, consistency and deployments before workload/team boundaries justify them. An undivided monolith hides ownership.

## Consequences

Shared releases and one database are intentional. Extract a module only for a demonstrated scaling or ownership requirement.

## Security implications

Module boundaries organize code; they are not process isolation. Architecture tests keep provider/web dependencies out of Domain/Application.

## Operational implications

Hosts may eventually scale separately while sharing rules and migrations. Phase 0 verifies startup and graceful shutdown only.
