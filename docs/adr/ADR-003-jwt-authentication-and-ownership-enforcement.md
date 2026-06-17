# ADR-003: Use JWT authentication with server-side ownership enforcement

- Status: Accepted
- Date: 2026-06-16

## Context

The exercise requires authorized and non-authorized endpoints. The application stores
private contacts per user, so authentication alone is not enough; the API must also ensure
that the authenticated caller cannot read or mutate another user's data by manipulating the
request.

## Decision

The API uses JWT bearer authentication. The authenticated user id is read from token
claims in the API layer and passed into application services as the effective owner id.

Ownership checks are enforced server-side for:

- listing contacts
- reading a contact
- creating, updating, and deleting a contact
- reading and updating account data
- changing a password

The frontend may guide the user, but it is never trusted for authorization decisions.

## Consequences

Positive:

- Prevents broken object-level authorization issues.
- Keeps ownership logic close to the use cases that need it.
- Makes the security story easy to explain in the interview.

Negative:

- Controllers and services must consistently propagate the authenticated user id.
- Tests must cover more forbidden-path scenarios.

## Notes

The API returns `403 Forbidden` when the caller is authenticated but not allowed to access
the targeted resource.
