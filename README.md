# ContactManager

Full-stack Contact Management application built with **.NET 10, Angular, PostgreSQL,
Clean Architecture, JWT Authentication and TDD** principles.

Built for a .NET technical interview exercise.

## User story

> As a sales rep, I want to manage my contacts (create, view, edit, delete) and log in
> securely, so my contact list stays current and private to me.

## Architecture

Clean Architecture with the dependency rule pointing inward:

```text
ContactManager.Domain          # entities — zero dependencies
ContactManager.Application     # business logic, validation, repository INTERFACES
ContactManager.Infrastructure  # raw ADO.NET (Npgsql) repositories, JWT, hashing
ContactManager.API             # ASP.NET Core controllers + DI composition root
web/                           # Angular 20 SPA
```

**Deliberate constraint (per the brief):** no Entity Framework, no Dapper, no MediatR.
All data access is hand-written, parameterized SQL via Npgsql.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (for PostgreSQL)
- [Node.js 22+](https://nodejs.org/) and Angular CLI 20 (for the frontend)

## Quick start

```bash
# 1. Start PostgreSQL (schema + seed data are applied automatically on first run)
docker compose up -d

# 2. Run the API
dotnet run --project src/ContactManager.API

# 3. Run the frontend (in another terminal)
cd web && npm start
```

### Demo credentials

| Username | Password   |
| -------- | ---------- |
| `demo`   | `Demo123!` |

The seed also creates a few demo contacts owned by this user.

## Development

```bash
dotnet build        # builds the .slnx solution (.NET 10 format)
dotnet test         # runs all xUnit test projects
```

The database runs in Docker; browse it with **pgAdmin**, **DBeaver**, or the VS Code
PostgreSQL extension:

```text
host: localhost   port: 5433
db:   contactmanager
user: contactmanager   password: contactmanager_dev_pwd
```

## Testing strategy (TDD)

Each layer has its own test project. Features are built test-first (red → green → refactor),
visible in the commit history:

- `Domain.Tests` — entity invariants / behavior
- `Application.Tests` — service logic + validation (repositories mocked with Moq)
- `Infrastructure.Tests` — Npgsql repositories against a test database
- `API.Tests` — endpoint integration tests via `WebApplicationFactory`

## AI-assisted development

This project was built with Claude Code as a *guided* collaborator. See:

- [`CLAUDE.md`](CLAUDE.md) — the context + guardrails given to the AI
- [`docs/genai/`](docs/genai/) — the prompts used, the generated output, and the critical
  evaluation / corrections (including the standalone Task-Management-API exercise)
- [`.claude/skills/add-feature-tdd/`](.claude/skills/add-feature-tdd/) — a custom skill
  encoding the TDD + Clean Architecture workflow as a reusable prompt
