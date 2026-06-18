# Contact Manager

A full-stack **Contact Manager** built for a .NET technical interview exercise:
an authenticated user manages their own private contacts (create, read, update, delete).

**Stack:** .NET 10 · ASP.NET Core Web API · PostgreSQL (raw ADO.NET / Npgsql) ·
Angular 20 · JWT auth · Clean Architecture · TDD · Docker.

---

## User story

> As an **account manager**, I want to register and securely access my account, so that I
> can manage my book of business contacts — adding clients and partners with their name,
> email and phone, quickly finding someone when I need to follow up, keeping details
> current as people change roles or companies, and removing contacts that are no longer
> relevant — ensuring my contact list stays organised, current, and private to my account.

This story drove the design end-to-end — each clause maps to a concrete decision:

| Story clause | What it drove |
| --- | --- |
| "register and securely access my account" | Register + login endpoints, JWT auth, PBKDF2 password hashing |
| "adding clients and partners with their name, email and phone" | Contact create with name, email, phone fields |
| "quickly finding someone when I need to follow up" | Client-side search, sortable columns, pagination with page-size |
| "keeping details current as people change roles or companies" | Edit contact + update profile/password (Accounts API) |
| "removing contacts that are no longer relevant" | Delete with confirmation |
| "private to my account" | Account-scoped queries; non-owned contact access is blocked server-side using the JWT owner id |

---

## How the exercise requirements are met

| Requirement (from the brief) | Where / how |
| --- | --- |
| Web app with API + data layer in .NET C# | ASP.NET Core Web API + a hand-written data layer |
| **No Entity Framework / Dapper / MediatR** | Data access is hand-written, parameterized SQL via **Npgsql** (raw ADO.NET) only |
| Database with a data table **and** a users table, each with a PK + ≥2 fields | `contacts` data table + `users`/`accounts` identity tables (see [Data model](#data-model)) |
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
  ContactManager.Domain/                  # business entities (AccountDomain, ContactDomain) + invariants — zero dependencies
  ContactManager.Application/             # services, validation, DTOs, repository INTERFACES; the auth UserModel lives here
  ContactManager.Infrastructure.Data/     # raw ADO.NET (Npgsql) repositories — all SQL here
  ContactManager.Infrastructure.Identity/ # JWT token generation, PBKDF2 password hashing
  ContactManager.Infrastructure.IoC/      # DI composition root — wires Infrastructure → Application
  ContactManager.API/                     # controllers, middleware, startup
tests/
  ContactManager.Domain.Tests/
  ContactManager.Application.Tests/
  ContactManager.Infrastructure.Tests/    # integration tests vs a real Postgres
  ContactManager.API.Tests/               # endpoint tests via WebApplicationFactory
db/init/                                  # schema + seed SQL (auto-applied on first container run)
web/                                      # Angular 20 SPA
docs/genai/                               # GenAI prompt log + critical evaluation
```

**Authentication vs. business identity.** `User` (login/credentials) is treated as an
*authentication* concern and modelled as an application-layer `UserModel` — deliberately
**not** a business entity. `AccountDomain` (the contact-book owner) is the *business*
identity and lives in the Domain layer with no knowledge of passwords or tokens. They share
one id, so the JWT subject identifies both. Password *behaviour* (hashing, token issuing)
stays behind `IPasswordHasher` / `ITokenGenerator` in `ContactManager.Application.Auth.Interfaces`,
implemented by `Infrastructure.Identity`, so the domain never touches auth logic and the
Application layer never depends on Infrastructure.

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

For a true first-clone walkthrough, see
[`docs/setup-from-scratch.md`](docs/setup-from-scratch.md).

---

## Quick start

Fresh clone:

```bash
git clone https://github.com/Ldoliveiradev/ContactManager.git
cd ContactManager
```

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

> Seed data note: schema + demo users are applied only on the first PostgreSQL start with
> an empty data volume. If you need to reset to the original seeded state, run
> `docker compose down -v` and then `docker compose up -d --build`.

### Option B — DB in Docker, API + frontend on your host (for debugging)

```bash
# 1. Start only PostgreSQL (schema + seed applied automatically on first run)
docker compose up -d postgres

# 2. Provide the API's local config (connection string + JWT secret).
#    These are NOT committed; create the file from the example and fill it in.
cp src/ContactManager.API/appsettings.Development.json.example \
   src/ContactManager.API/appsettings.Development.json
#    Set ConnectionStrings:Postgres to Host=localhost;Port=5433 and a 32+ char Jwt:Secret.

# 3. Run the API — it listens on http://localhost:5050
dotnet run --project src/ContactManager.API

# 4. Run the Angular dev server (another terminal)
cd web && npm install     # first time only
npm start                 # http://localhost:4200
```

> The Angular dev server (`:4200`) can call **either** API — both allow CORS from
> `http://localhost:4200`. Set `apiUrl` in
> [`web/src/environments/environment.ts`](web/src/environments/environment.ts) to
> `http://localhost:5050/api/v1` for the host-run API, or keep `http://localhost:8085/api/v1`
> to reuse the Docker API (then you can skip steps 2–3). See
> [`docs/setup-from-scratch.md`](docs/setup-from-scratch.md#option-b--run-the-app-locally-on-the-host)
> for the full walkthrough.

---

## Demo credentials

The database is **seeded on first run** with two demo users and their own contacts.

**App logins** (sign in at the frontend — the value is a **username, not an email**):

| Username | Password   | Notes                                                       |
| -------- | ---------- | ----------------------------------------------------------- |
| `demo`   | `Demo123!` | Owns **15** contacts — good for search / sort / pagination. |
| `sales2` | `Demo123!` | Owns **3** different contacts.                              |

Two users are seeded so you can demonstrate **data isolation**: log in as `demo`, then as
`sales2`, and each sees only their own contacts. Direct access to a non-owned contact is
rejected by the API, and the other user's data is never modified. You can also click
**Register** to create a fresh account.

**pgAdmin login** (<http://localhost:5051>):

| Email                      | Password |
| -------------------------- | -------- |
| `admin@contactmanager.com` | `admin`  |

Inside pgAdmin, register the server with **Host `postgres`, Port `5432`** (the internal
service name — not `localhost:5433`), database `contactmanager`, user `contactmanager`,
password `contactmanager_dev_pwd`.

---

## Data model

Authentication is modelled as a separate concern from the business identity:

- **`users`** — the *authentication* identity (login). Holds the credentials only.
- **`accounts`** — the *business* identity (the account manager who owns a contact book).
- **`contacts`** — a contact in an account's book.

A user and their account are **1:1 and share the same `id`** (set together at registration),
so `accounts.id` is also the foreign key back to `users.id`. This keeps the login mechanism
(`users`, in the Identity/Application layer) out of the business domain (`accounts`, in the
Domain layer) without an extra surrogate key or join column.

```text
users  (auth)              accounts  (business)        contacts
-----                      --------                    --------
id            UUID  PK ◄── id         UUID  PK  ◄───── account_id  UUID  FK
username      VARCHAR      first_name VARCHAR          id          UUID  PK
password_hash VARCHAR      last_name  VARCHAR          name        VARCHAR
created_at    TIMESTAMPTZ  email      VARCHAR UNIQUE   email       VARCHAR
updated_at    TIMESTAMPTZ  created_at TIMESTAMPTZ      phone       VARCHAR  (nullable)
                           updated_at TIMESTAMPTZ      created_at  TIMESTAMPTZ
                                                       updated_at  TIMESTAMPTZ
```

> - `accounts.id REFERENCES users(id)` — the shared primary key enforces the 1:1 link
>   (the `◄──` between `users.id` and `accounts.id`).
> - `contacts.account_id REFERENCES accounts(id)` (`ON DELETE CASCADE`) scopes every
>   contact to exactly one account.

Passwords are stored as **PBKDF2 (HMAC-SHA256, 100k iterations, per-password random salt)**
hashes — never plaintext.

---

## API reference

Base URL: `http://localhost:8085` (Docker) — all routes are under `/api/v1`.

### Auth API (anonymous — the "non-authorized" endpoints)

| Method | Route                   | Body                                                         | Success              | Errors                              |
| ------ | ----------------------- | ------------------------------------------------------------ | -------------------- | ----------------------------------- |
| POST   | `/api/v1/auth/register` | `{ username, firstName, lastName, email, password }`         | `201`                | `400` invalid, `409` username/email |
| POST   | `/api/v1/auth/login`    | `{ username, password }`                                     | `200` + `{ token }`  | `401` invalid credentials           |

### Contacts API (`[Authorize]` — the "authorized" endpoints, JWT bearer required)

| Method | Route                   | Body                      | Success                   | Errors                    |
| ------ | ----------------------- | ------------------------- | ------------------------- | ------------------------- |
| GET    | `/api/v1/contacts`      | —                         | `200` (caller's contacts) | `401`                     |
| GET    | `/api/v1/contacts/{id}` | —                         | `200`                     | `401`, `403`, `404`      |
| POST   | `/api/v1/contacts`      | `{ name, email, phone? }` | `201`                     | `400`, `401`             |
| PUT    | `/api/v1/contacts/{id}` | `{ name, email, phone? }` | `200`                     | `400`, `401`, `403`, `404` |
| DELETE | `/api/v1/contacts/{id}` | —                         | `204`                     | `401`, `403`, `404`      |

**Security notes:**

- **All contact endpoints require authentication** (`[Authorize]`); without a token they
  return `401`.
- The owning user id is read from the **JWT claims**, never from the request body or route.
  Create/update DTOs don't even have a user-id field, so ownership **cannot be spoofed** —
  extra fields in the body are ignored and the caller is always the owner.
- A contact owned by another user is blocked by the API: **read, update, and delete return
  `403`** for ownership mismatch, and the other user's data is never modified.
- Login returns the same `401` for unknown-user and wrong-password (no user enumeration).
- These rules are enforced **server-side** in the Application/Domain layers (the UI also
  validates for UX, but the API is authoritative). Errors use RFC 7807 `ProblemDetails`.

### Accounts API (`[Authorize]` — profile management)

| Method | Route                          | Body                                                                 | Success | Errors              |
| ------ | ------------------------------ | -------------------------------------------------------------------- | ------- | ------------------- |
| GET    | `/api/v1/accounts`             | —                                                                    | `200`   | `401`               |
| PUT    | `/api/v1/accounts`             | `{ firstName, lastName, email }`                                     | `200`   | `400`, `401`        |
| POST   | `/api/v1/auth/change-password` | `{ userId, currentPassword, newPassword, confirmNewPassword }`       | `204`   | `400`, `401`, `403` |

> **On roles/claims:** the brief asks for *authorized vs non-authorized* endpoints
> (authentication), which is implemented. Role-based authorization was intentionally left
> out as it isn't required; the JWT already carries `sub` (user id) and `unique_name`
> claims, and the design extends trivially to roles (add a `role` column → claim →
> `[Authorize(Roles = …)]`) if ever needed.

---

## Frontend

Angular 20 SPA (`web/`) — standalone components, signals, lazy-loaded routes.

**Features:** home page, login / registration, full contact CRUD, client-side search,
sortable columns, pagination with a page-size selector, profile/password management,
dedicated `401` / `404` / generic error pages, light/dark theme (follows OS preference,
manual toggle, persisted), and responsive (mobile-first) layouts.

**Structure:**

```text
web/src/app/
  core/          JWT interceptor, auth guard, theme service
  features/
    auth/        login + register (models, service, component)
    contacts/    contact CRUD (models, service, list + form components)
    accounts/    profile + password management (models, service, component)
    system/      home, unauthorized, not-found, and generic-error pages
  shared/
    ui/          ui-button, ui-card, ui-alert, ui-form-field, ui-pagination,
                 ui-data-view (generic searchable/sortable/paginated grid),
                 ui-skeleton (shimmer placeholder for loading states)
    pipes/       PhonePipe
    directives/  AutofocusDirective
    validators/  contact field validators
  styles/        SCSS design system: tokens, mixins, breakpoints
```

- JWT is attached by an HTTP interceptor; authenticated API `401` responses trigger
  auto-logout + redirect to the `401` page.
- API `404` responses route to the not-found page; `5xx`/network failures route to the
  generic error page.
- Routes are protected by an auth guard.
- Frontend API-facing interfaces mirror the backend transport contracts (for example
  `PaginationFilter`, `PaginationResponse<T>`, `AccountResponse`, `ContactResponse`).
- Styling uses a small **SCSS design system** (design tokens surfaced as themeable CSS
  custom properties) — no UI framework dependency. Icons via FontAwesome; font is Inter.

---

## Testing

The project is tested at every level (TDD — tests written first, visible in history).

| Suite | Tooling | Covers |
| ----- | ------- | ------ |
| Backend unit/integration | xUnit · Moq · FluentAssertions | Domain invariants, Application services + validation, Npgsql repositories (vs a real Postgres), API endpoints via `WebApplicationFactory` |
| Frontend unit | Karma · Jasmine | Services, guard, interceptor, feature components, system pages, shared components, pipe, directive, validators |
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

> The Infrastructure/API integration tests run against dedicated `contactmanager_test` /
> `contactmanager_test_api` databases (created automatically). They **fail** (rather than
> silently skip) if Postgres is unreachable, so a missing database can never hide an unrun
> suite — the CI workflow provisions a Postgres service container for them.

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

---

## Interview support docs

For the presentation and code review, these focused docs are available:

- [`docs/setup-from-scratch.md`](docs/setup-from-scratch.md) — fresh-clone setup guide
- [`docs/adr/`](docs/adr/) — architectural decision records
- [`docs/architecture/`](docs/architecture/) — C4-style architecture and sequence diagrams
- [`docs/presentation/user-story.md`](docs/presentation/user-story.md) — the informal user story
- [`docs/presentation/tdd-approach.md`](docs/presentation/tdd-approach.md) — testing strategy
- [`docs/presentation/genai-usage.md`](docs/presentation/genai-usage.md) — how GenAI was used and validated
- [`docs/presentation/demo-script.md`](docs/presentation/demo-script.md) — a concise live demo flow
