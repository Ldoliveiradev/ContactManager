# ADR-004: Mirror backend API contracts in the Angular frontend

- Status: Accepted
- Date: 2026-06-16

## Context

The frontend consumes several request and response payloads from the backend. If the UI
uses ad-hoc shapes that drift from backend contracts, integration bugs become harder to
spot and serialization expectations become implicit.

## Decision

Angular feature folders define request and response interfaces that intentionally mirror the
backend transport contracts, including pagination envelopes and feature-specific DTOs.

Examples:

- `PaginationFilter`
- `PaginationResponse<T>`
- `ContactResponse`
- `AccountResponse`
- auth request/response interfaces

## Consequences

Positive:

- Contract changes are more visible in the UI codebase.
- Frontend services stay explicit about what the API returns.
- Feature components can consume typed transport models consistently.

Negative:

- Some interfaces look repetitive because they intentionally mirror backend DTOs.
- Contract changes require updates in both backend and frontend.

## Notes

This decision is about transport consistency, not about sharing compiled models across
projects. The backend remains the source of truth.
