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

### Option A — everything in Docker (recommended, one command)

```bash
docker compose up -d --build
```

Brings up the whole stack: **frontend + API + PostgreSQL + pgAdmin**. On the Docker network
the API reaches the database by its service name (`Host=postgres`) and the frontend's nginx
proxies `/api` to the API container — so no host ports or CORS are involved between them.

| Service  | URL                              | Notes                                |
| -------- | -------------------------------- | ------------------------------------ |
| Frontend | <http://localhost:4300>          | the app — start here                 |
| API      | <http://localhost:8085>          | direct access (Swagger at `/openapi`)|
| pgAdmin  | <http://localhost:5051>          | DB browser (see logins below)        |
| Postgres | `localhost:5433`                 | for external tools (DBeaver, etc.)   |

> Ports use non-default host values (4300 / 8085 / 5433 / 5051) to avoid colliding with
> other local services. Change the left-hand side of each `ports:` mapping in
> `docker-compose.yml` if needed.

### Option B — DB in Docker, API + frontend on your host (handy for debugging)

```bash
# 1. Start only PostgreSQL (schema + seed applied automatically on first run)
docker compose up -d postgres

# 2. Run the API on your machine (uses Host=localhost:5433 from appsettings.json)
dotnet run --project src/ContactManager.API

# 3. Run the frontend dev server (in another terminal)
cd web && npm install   # first time only
npm start               # http://localhost:4200
```

The SPA (Angular 20, standalone components + signals) provides login/registration and full
contact CRUD, with light/dark theming, a JWT HTTP interceptor, and route guards. In dev it
calls the API URL in
[`web/src/environments/environment.ts`](web/src/environments/environment.ts).

## Test users & logins

**App login (use these to sign in at the frontend):**

| Username | Password   | Notes                                                    |
| -------- | ---------- | -------------------------------------------------------- |
| `demo`   | `Demo123!` | Seeded user. **Username, not an email.** Has 3 contacts. |

> You can also click **Register** on the login screen to create your own account.

**pgAdmin login** (<http://localhost:5051>):

| Email                      | Password |
| -------------------------- | -------- |
| `admin@contactmanager.com` | `admin`  |

Inside pgAdmin, register the server with **Host `postgres`, Port `5432`**, database
`contactmanager`, user `contactmanager`, password `contactmanager_dev_pwd`.
(Use `postgres:5432` — the internal service name — not `localhost:5433`.)

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
