---
name: add-feature-tdd
description: Add a new vertical feature slice to the Contact Manager using strict TDD and Clean Architecture. Use when the user asks to add an endpoint, service, repository, or domain rule. Enforces the no-EF/Dapper/MediatR constraint and the red-green-refactor cycle.
---

# Add Feature (TDD + Clean Architecture)

Encodes this project's required workflow so every feature is built test-first, respects the
layer boundaries, and never violates the graded constraints. Read `CLAUDE.md` first.

## Non-negotiable constraints

- **No Entity Framework, no Dapper, no MediatR.** Data access = hand-written SQL via Npgsql.
- **Dependency rule:** Domain ← Application ← Infrastructure / API. SQL lives ONLY in
  Infrastructure. Business rules/validation live ONLY in Application.
- **Test-first.** Write the failing test, watch it fail for the right reason, then implement.

## Workflow

For a feature touching the domain + service + repository + endpoint, proceed in this order,
committing at each red→green step so the TDD history is visible:

1. **Domain (if needed).** Add/extend the entity in `ContactManager.Domain`. Keep invariants
   enforced (private setters, intention-revealing methods). Add a `Domain.Tests` test first
   if there's behavior (not just data).

2. **Application — interface + service, test-first.**
   - In `ContactManager.Application.Tests`, write the failing test for the new service method
     (mock the repository interface with Moq, assert with FluentAssertions).
   - Add/extend the repository **interface** in `Application` (no implementation).
   - Implement the service method with validation. Run the test → green.

3. **Infrastructure — repository, test-first.**
   - In `ContactManager.Infrastructure.Tests`, write a test against the Npgsql repository
     (use a disposable test database / transaction, not the dev DB).
   - Implement the method with **parameterized** hand-written SQL. No string concatenation.

4. **API — controller, integration test-first.**
   - In `ContactManager.API.Tests`, write a `WebApplicationFactory` integration test asserting
     the correct status code (201/200/204/404/400/401/403) and payload.
   - Add the controller action. Wire DI in the composition root only.

5. **Frontend (if asked).** Add the Angular service call + component in `web/`, keeping
   state and components organized.

6. **Document.** If a non-obvious decision was made, add a short note under `docs/genai/`.

## Checklist before declaring done

- [ ] A failing test existed before each implementation.
- [ ] `dotnet test` passes; `dotnet build` has 0 warnings.
- [ ] No EF/Dapper/MediatR introduced.
- [ ] SQL is parameterized; ownership/authorization checked where data is user-scoped.
- [ ] Correct HTTP verb + status codes.
- [ ] Conventional-commit messages, small commits.
