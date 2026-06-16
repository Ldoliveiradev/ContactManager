# CLAUDE.md — Project Context for AI-Assisted Development

This file gives Claude Code (and any GenAI tool) the context and guardrails needed to
contribute to this codebase correctly. It is intentionally checked in: it documents how
AI was used as a *guided collaborator*, not an unguided code generator.

## What this project is

A full-stack **Contact Manager**: an authenticated user manages their own contacts
(create / read / update / delete). Built for a .NET technical interview exercise.

**User story (drives the work):**
> As a sales rep, I want to manage my contacts (create, view, edit, delete) and log in
> securely, so my contact list stays current and private to me.

## Tech stack

- **Backend:** .NET 10, ASP.NET Core Web API, C#
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure / API)
- **Data:** PostgreSQL via **raw ADO.NET (Npgsql)** — hand-written SQL only
- **Auth:** JWT bearer tokens
- **Frontend:** Angular 20 (`web/`)
- **Tests:** xUnit + Moq + FluentAssertions (v7, last Apache-licensed), TDD
- **Infra:** Docker Compose for PostgreSQL
- **Solution format:** `.slnx` (.NET 10 default)

## HARD CONSTRAINTS (from the exercise brief — do not violate)

1. **NO Entity Framework, NO Dapper, NO MediatR.** Data access is hand-written SQL with
   Npgsql `NpgsqlConnection` / `NpgsqlCommand`. This is a graded constraint.
2. **Clean Architecture dependency rule:** dependencies point inward.
   - `Domain` depends on nothing.
   - `Application` depends only on `Domain`. Defines repository **interfaces**.
   - `Infrastructure` implements those interfaces (the only place SQL lives).
   - `API` depends on `Application` (+ `Infrastructure` only at the DI composition root).
   - Business logic/validation lives in `Application`, independent of data + API.
3. **TDD:** write a failing test first, then the implementation. Keep commits small so the
   red → green → refactor history is visible.

## Project layout

```text
src/
  ContactManager.Domain          # entities, value objects — zero dependencies
  ContactManager.Application     # services, validation, repository interfaces, DTOs
  ContactManager.Infrastructure  # Npgsql repositories (raw SQL), JWT, password hashing
  ContactManager.API             # controllers, DI wiring, middleware
tests/
  ContactManager.Domain.Tests
  ContactManager.Application.Tests
  ContactManager.Infrastructure.Tests
  ContactManager.API.Tests       # integration tests via WebApplicationFactory
db/init/                         # schema + seed SQL, auto-applied by Postgres container
web/                             # Angular 20 frontend
docs/genai/                      # AI prompt log + critical-evaluation notes (graded)
```

## Conventions

- Commit messages: Conventional Commits (`feat:`, `test:`, `fix:`, `refactor:`, `chore:`).
- One vertical slice per feature branch (`feature/contact-crud`, `feature/auth-jwt`).
- Controllers depend on Application interfaces, never on Infrastructure types directly.
- All SQL is parameterized (no string concatenation) — SQL injection safe.

## Common commands

```bash
docker compose up -d            # start PostgreSQL (applies db/init on first run)
dotnet build                    # builds the .slnx
dotnet test                     # runs all test projects
dotnet run --project src/ContactManager.API
cd web && npm start             # Angular dev server
```

## For the AI assistant

When asked to add a feature, default to: **write the failing test first**, confirm it
fails for the right reason, then implement the minimum to pass, then refactor. Never
introduce EF/Dapper/MediatR. Keep the layer boundaries clean. When you make a non-obvious
decision, note it in `docs/genai/` so it can be explained during the code review.
