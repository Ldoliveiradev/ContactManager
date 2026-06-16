# Contact Manager

A full-stack **Contact Manager** built for a .NET technical interview exercise:
an authenticated user manages their own private contacts (create, read, update, delete).

**Stack:** .NET 10 · ASP.NET Core Web API · PostgreSQL (raw ADO.NET / Npgsql) ·
Angular 20 · JWT auth · Clean Architecture · TDD · Docker.

---

## User story

> As a sales rep, I want to manage my contacts (create, view, edit, delete) and log in
> securely, so my contact list stays current and private to me.

This drives the whole design: contacts are **user-scoped** — every user sees and edits only
their own.

---

## How the exercise requirements are met

| Requirement (from the brief) | Where / how |
| --- | --- |
| Web app with API + data layer in .NET C# | ASP.NET Core Web API + a hand-written data layer |
| **No Entity Framework / Dapper / MediatR** | Data access is hand-written, parameterized SQL via **Npgsql** (raw ADO.NET) only |
| Database with a data table **and** a users table, each with a PK + ≥2 fields | `contacts` and `users` tables (see [Data model](#data-model)) |
| CRUD API with correct HTTP verbs / status codes | `ContactsController` — see [API reference](#api-reference) |
| Second API: user creation, login, **authorized + non-authorized** endpoints | `AuthController` (anonymous) + `ContactsController` (`[Authorize]`) |
| Business logic layer, independent of data + API | `ContactManager.Application` (services, validation, interfaces) |
| Unit tests for data, business, and API layers | xUnit across all layers + Angular unit tests + Playwright E2E |
| TDD preferred | Every feature built test-first; visible in the commit history |
| Frontend in a framework, responsive, CRUD, structured | Angular 20 SPA — see [Frontend](#frontend) |
| README + seeded demo data / credentials | This file; the DB is auto-seeded ([demo login](#demo-credentials)) |
| GenAI usage documented | [`docs/genai/`](docs/genai/) + [`CLAUDE.md`](CLAUDE.md) |

---

## Architecture

Clean Architecture — dependencies point **inward**, so business rules never depend on
infrastructure or the web layer:

```text
src/
  ContactManager.Domain          # entities + invariants — zero dependencies
  ContactManager.Application     # services, validation, DTOs, repository INTERFACES
  ContactManager.Infrastructure  # raw ADO.NET (Npgsql) repositories, JWT, PBKDF2 hashing
  ContactManager.API             # controllers, middleware, DI composition root
tests/
  ContactManager.Domain.Tests
  ContactManager.Application.Tests
  ContactManager.Infrastructure.Tests   # integration tests vs a real Postgres
  ContactManager.API.Tests              # endpoint tests via WebApplicationFactory
db/init/                         # schema + seed SQL (auto-applied by the Postgres container)
web/                             # Angular 20 SPA
docs/genai/                      # GenAI prompt log + critical evaluation
```

Key rules enforced:

- `Domain` depends on nothing. `Application` depends only on `Domain` and defines the
  repository **interfaces**. `Infrastructure` implements them (the only place SQL lives).
- Controllers depend on Application interfaces, never on Infrastructure types directly
  (Infrastructure is referenced by the API only at the DI composition root).
- All SQL is **parameterized** — no string concatenation (SQL-injection safe).

---

## Prerequisites

- [Docker](https://www.docker.com/) — runs the whole stack
- For local (non-Docker) development:
  [.NET 10 SDK](https://dotnet.microsoft.com/) and [Node.js 22+](https://nodejs.org/)

---

## Quick start

### Option A — everything in Docker (recommended, one command)

```bash
docker compose up -d --build
```

Brings up **frontend + API + PostgreSQL + pgAdmin**. On the Docker network the API reaches
the database by its service name (`Host=postgres`) and the frontend's nginx proxies `/api`
to the API container, so no host ports or CORS are involved between them.

| Service  | URL                     | Notes                                         |
| -------- | ----------------------- | --------------------------------------------- |
| Frontend | <http://localhost:4300> | **the app — start here**                      |
| API      | <http://localhost:8085> | direct access; OpenAPI at `/openapi/v1.json`  |
| pgAdmin  | <http://localhost:5051> | DB browser (see [logins](#demo-credentials))  |
| Postgres | `localhost:5433`        | for external tools (DBeaver, etc.)            |

> Host ports are non-default (4300 / 8085 / 5433 / 5051) to avoid colliding with other
> local services. Change the left side of each `ports:` mapping in `docker-compose.yml`
> if needed.

### Option B — DB in Docker, API + frontend on your host (for debugging)

```bash
# 1. Start only PostgreSQL (schema + seed applied automatically on first run)
docker compose up -d postgres

# 2. Run the API (uses Host=localhost:5433 from appsettings.json)
dotnet run --project src/ContactManager.API

# 3. Run the Angular dev server (another terminal)
cd web && npm install     # first time only
npm start                 # http://localhost:4200
```

---

## Demo credentials

The database is **seeded on first run** with a demo user and 15 contacts.

**App login** (sign in at the frontend):

| Username | Password   | Notes                                                |
| -------- | ---------- | ---------------------------------------------------- |
| `demo`   | `Demo123!` | A **username, not an email**. Owns 15 seeded contacts. |

You can also click **Register** on the login screen to create a fresh account.

**pgAdmin login** (<http://localhost:5051>):

| Email                      | Password |
| -------------------------- | -------- |
| `admin@contactmanager.com` | `admin`  |

Inside pgAdmin, register the server with **Host `postgres`, Port `5432`** (the internal
service name — not `localhost:5433`), database `contactmanager`, user `contactmanager`,
password `contactmanager_dev_pwd`.

---

## Data model

Two tables, each with a primary key and additional fields, per the brief:

```text
users                              contacts
-----                              --------
id            UUID  PK            id          UUID  PK
username      VARCHAR  UNIQUE     user_id     UUID  FK -> users(id) ON DELETE CASCADE
password_hash VARCHAR            name        VARCHAR
created_at    TIMESTAMPTZ        email       VARCHAR
                                 phone       VARCHAR  (nullable)
                                 created_at  TIMESTAMPTZ
                                 updated_at  TIMESTAMPTZ
```

Passwords are stored as **PBKDF2 (HMAC-SHA256, 100k iterations, per-password random salt)**
hashes — never plaintext.

---

## API reference

Base URL: `http://localhost:8085` (Docker) — all routes are under `/api`.

### Auth API (anonymous — the "non-authorized" endpoints)

| Method | Route                | Body                     | Success             | Errors                              |
| ------ | -------------------- | ------------------------ | ------------------- | ----------------------------------- |
| POST   | `/api/auth/register` | `{ username, password }` | `201`               | `400` invalid, `409` username taken |
| POST   | `/api/auth/login`    | `{ username, password }` | `200` + `{ token }` | `401` invalid credentials           |

### Contacts API (`[Authorize]` — the "authorized" endpoints, JWT bearer required)

| Method | Route                | Body                      | Success                   | Errors              |
| ------ | -------------------- | ------------------------- | ------------------------- | ------------------- |
| GET    | `/api/contacts`      | —                         | `200` (caller's contacts) | `401`               |
| GET    | `/api/contacts/{id}` | —                         | `200`                     | `401`, `404`        |
| POST   | `/api/contacts`      | `{ name, email, phone? }` | `201`                     | `400`, `401`        |
| PUT    | `/api/contacts/{id}` | `{ name, email, phone? }` | `200`                     | `400`, `401`, `404` |
| DELETE | `/api/contacts/{id}` | —                         | `204`                     | `401`, `404`        |

**Security notes:**

- The owning user id is read from the **JWT claims**, never from the request body or route.
- A contact owned by another user returns **`404` (not `403`)** so ownership isn't leaked
  (IDOR protection).
- Login returns the same `401` for unknown-user and wrong-password (no user enumeration).
- Errors are returned as RFC 7807 `ProblemDetails`.

---

## Frontend

Angular 20 SPA (`web/`) — standalone components, signals, lazy-loaded routes.

**Features:** login / registration, full contact CRUD, client-side search, sortable
columns, pagination with a page-size selector, light/dark theme (follows OS preference,
manual toggle, persisted), and responsive (mobile-first) layouts.

**Structure:**

```text
web/src/app/
  core/          services (auth, contacts, theme), JWT interceptor, auth guard, models
  features/      auth (login) and contacts (list, form) feature components
  shared/        reusable UI: ui-button, ui-card, ui-alert, ui-form-field,
                 ui-pagination, ui-data-view (generic searchable/sortable/paginated
                 collection); PhonePipe; AutofocusDirective; form validators
  styles/        SCSS design system: tokens, mixins, breakpoints
```

- JWT is attached by an HTTP interceptor; a `401` triggers auto-logout + redirect.
- Routes are protected by an auth guard.
- Styling uses a small **SCSS design system** (design tokens surfaced as themeable CSS
  custom properties) — no UI framework dependency. Icons via FontAwesome; font is Inter.

---

## Testing

The project is tested at every level (TDD — tests written first, visible in history).

| Suite | Tooling | Covers |
| ----- | ------- | ------ |
| Backend unit/integration | xUnit · Moq · FluentAssertions | Domain invariants, Application services + validation, Npgsql repositories (vs a real Postgres), API endpoints via `WebApplicationFactory` |
| Frontend unit | Karma · Jasmine | Services, guard, interceptor, shared components, pipe, directive, validators |
| End-to-end | Playwright | Full user flows against the running stack: auth, CRUD, validation, search/sort/pagination, dark mode |

### Run the tests

```bash
# Backend (needs Docker Postgres up for the integration tests)
docker compose up -d postgres
dotnet test

# Frontend unit tests (headless)
cd web && npm run test:ci

# End-to-end (needs the full stack up)
docker compose up -d --build
cd web && npm run e2e
```

> The Infrastructure/API integration tests run against a dedicated `contactmanager_test`
> database (created automatically); they skip gracefully if Postgres isn't reachable.

---

## Common commands

```bash
docker compose up -d --build     # run the whole stack
docker compose down              # stop it (add -v to also wipe the DB volume)
dotnet build                     # build the .slnx solution (.NET 10 format)
dotnet test                      # all backend test projects
dotnet run --project src/ContactManager.API
cd web && npm start              # Angular dev server (http://localhost:4200)
cd web && npm run test:ci        # frontend unit tests
cd web && npm run e2e            # Playwright E2E
```

---

## AI-assisted development (GenAI deliverable)

This project was built with Claude Code as a **guided** collaborator, not an unguided
generator. The evidence the brief asks for:

- [`CLAUDE.md`](CLAUDE.md) — the context + hard constraints given to the AI.
- [`docs/genai/`](docs/genai/) — the prompts used, the generated output, and the **critical
  evaluation / corrections**, including the standalone *task-management-API* exercise from
  the brief (with the security bug the AI missed and how it was fixed).
- [`.claude/skills/add-feature-tdd/`](.claude/skills/add-feature-tdd/) — a custom skill
  encoding the TDD + Clean Architecture workflow as a reusable prompt.
