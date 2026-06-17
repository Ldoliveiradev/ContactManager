# ADR-001: Use Clean Architecture boundaries

- Status: Accepted
- Date: 2026-06-16

## Context

The exercise explicitly asks for Clean Architecture, separation of concerns, and a
business logic layer independent from the API and data access layers. The project also
needs to stay small enough to present clearly in an interview.

## Decision

The solution is split into these main layers:

- `ContactManager.Domain`: business entities and invariants
- `ContactManager.Application`: use-case services, validators, DTOs, and interfaces
- `ContactManager.Infrastructure.Data`: repository implementations and SQL access
- `ContactManager.Infrastructure.Identity`: password hashing and JWT generation
- `ContactManager.API`: HTTP endpoints, auth wiring, and composition root
- `web/`: Angular frontend

Dependencies point inward. `Domain` depends on nothing. `Application` depends on
`Domain`. Infrastructure implements application interfaces. The API depends on
Application contracts and wires Infrastructure through DI.

## Consequences

Positive:

- Business rules can be tested without HTTP or database concerns.
- SQL and security infrastructure are isolated from core use cases.
- The interview panel can inspect layer boundaries quickly.

Negative:

- More projects and interfaces than a simpler layered CRUD app.
- Some mappings and DTOs exist mainly to preserve boundaries.

## Notes

This project intentionally keeps the architecture pragmatic. It does not add CQRS,
MediatR, or domain event infrastructure because the exercise does not require that
complexity.
