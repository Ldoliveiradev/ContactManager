# ADR-002: Use raw SQL repositories instead of an ORM

- Status: Accepted
- Date: 2026-06-16

## Context

The exercise forbids Entity Framework, Dapper, and MediatR. The application still needs a
real persistence layer, testable CRUD behavior, and a clear explanation of how data access
works.

## Decision

Data access is implemented with hand-written, parameterized SQL using `Npgsql` and
ADO.NET primitives. Repository interfaces live in `ContactManager.Application`, and their
implementations live in `ContactManager.Infrastructure.Data`.

## Consequences

Positive:

- Fully compliant with the exercise constraints.
- SQL stays explicit and easy to review during code review.
- No hidden ORM behavior around tracking, lazy loading, or generated queries.

Negative:

- More manual mapping code.
- Pagination, filtering, and updates require explicit SQL maintenance.
- Developers must be disciplined about parameterization and query consistency.

## Notes

The codebase avoids string-concatenated SQL and uses parameters to reduce injection risk.
Integration tests cover repository behavior against a real PostgreSQL instance.
