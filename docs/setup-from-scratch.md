# Setup From Scratch

This guide is for someone cloning the repository for the first time and wanting the
shortest path to a working demo. It covers **two ways to run the project**:

- **[Option A — Docker](#option-a--run-everything-with-docker-recommended)** (recommended): the
  whole stack (PostgreSQL, API, frontend, pgAdmin) in containers, one command.
- **[Option B — Local](#option-b--run-the-app-locally-on-the-host)**: PostgreSQL in Docker, but the
  API and frontend run as processes on your host — best for active development and debugging.

## 1. Clone the repository

```bash
git clone https://github.com/Ldoliveiradev/ContactManager.git
cd ContactManager
```

## Option A — Run everything with Docker (recommended)

Prerequisite:

- Docker Desktop or Docker Engine with Compose support

Start everything:

```bash
docker compose up -d --build
```

Then open:

- Frontend: `http://localhost:4300`
- API: `http://localhost:8085`
- pgAdmin: `http://localhost:5051`

Demo credentials are in the root [`README.md`](../README.md#demo-credentials).

That's it — the database is seeded automatically on first start. Skip to
[§ Verify the app works](#3-verify-the-app-works).

## Option B — Run the app locally on the host

Use this when you want to edit, run, and debug the API or frontend directly.
PostgreSQL still runs in Docker; only the API and the Angular dev server run on your host.

Prerequisites:

- .NET 10 SDK
- Node.js 22+
- Docker (for PostgreSQL only)

### B.1 Start PostgreSQL

```bash
docker compose up -d postgres
```

This publishes Postgres on host port `5433` and seeds schema + demo data on first run.

### B.2 Configure the API

The API has **no connection string or JWT secret checked in** — it reads them from a local,
git-ignored `appsettings.Development.json`. Create it from the provided example:

```bash
cp src/ContactManager.API/appsettings.Development.json.example \
   src/ContactManager.API/appsettings.Development.json
```

Then edit the new file and fill in the two placeholders:

```jsonc
{
  "ConnectionStrings": {
    // Host-run API reaches the Dockerized Postgres on host port 5433.
    "Postgres": "Host=localhost;Port=5433;Database=contactmanager;Username=contactmanager;Password=contactmanager_dev_pwd"
  },
  "Jwt": {
    "Secret": "a-local-dev-signing-secret-at-least-32-characters-long"
  }
}
```

> The DB password above (`contactmanager_dev_pwd`) is the default from
> [`docker-compose.yml`](../docker-compose.yml). The JWT secret must be at least 32 characters.
> If either value is missing the API throws on startup with a clear "Missing …" message.

### B.3 Run the API

```bash
dotnet restore
dotnet run --project src/ContactManager.API
```

The host-run API listens on **`http://localhost:5050`** (see
[`launchSettings.json`](../src/ContactManager.API/Properties/launchSettings.json)) — **not**
`8085`, which is the Docker API.

### B.4 Point the frontend at an API

The Angular dev server on `:4200` can call **either** API — they both allow CORS from
`http://localhost:4200` (configured in [`Program.cs`](../src/ContactManager.API/Program.cs)).
You only change one line, `apiUrl`, in
[`web/src/environments/environment.ts`](../web/src/environments/environment.ts):

| You want `:4200` to call…       | Set `apiUrl` to                  | When                                          |
| ------------------------------- | -------------------------------- | --------------------------------------------- |
| **Docker API** (`:8085`)        | `http://localhost:8085/api/v1`   | Default — only the DB+API in Docker, UI local |
| **Host-run API** (`:5050`, B.3) | `http://localhost:5050/api/v1`   | Debugging the API locally too                 |

```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5050/api/v1', // or :8085 to use the Docker API
};
```

> The Docker API's `:8085` is published to your host, so the browser can reach it from
> `:4200` directly — no proxy needed. (If you only want to run the UI locally and reuse
> the Docker API, you can skip B.2 and B.3 entirely and just point `apiUrl` at `:8085`.)

### B.5 Run the frontend

```bash
cd web
npm install
npm start
```

Then open:

- Frontend: `http://localhost:4200` (the API already allows CORS from this origin)

## 2. Resetting seeded data

The PostgreSQL container seeds schema and demo data **only when the database volume is empty**.

If you have run the project before and want a clean seeded database again:

```bash
docker compose down -v
docker compose up -d --build
```

That removes the existing Postgres volume and recreates the database from the scripts in
[`db/init/`](../db/init/).

## 3. Verify the app works

1. Open the frontend (`:4300` for Docker, `:4200` for local).
2. Sign in with one of the seeded users from the root [`README.md`](../README.md#demo-credentials).
3. Confirm contacts load.
4. Create, edit, and delete a contact.
5. Open the profile page and verify account details load.

## 4. Run tests

Backend (needs Postgres for the integration tests):

```bash
docker compose up -d postgres
dotnet test
```

Frontend unit tests:

```bash
cd web
npm install
npm run test:ci
```

End-to-end (runs against the full Dockerized stack on `:4300`):

```bash
docker compose up -d --build
cd web
npm install
npm run e2e
```

## 5. Common first-time issues

### Port already in use

The project uses these host ports:

| Port   | Used by                          |
| ------ | -------------------------------- |
| `4300` | frontend (Docker)                |
| `4200` | frontend (local `npm start`)     |
| `8085` | API (Docker)                     |
| `5050` | API (local `dotnet run`)         |
| `5433` | PostgreSQL                       |
| `5051` | pgAdmin                          |

If a Docker port is busy, change the host side of the mapping in
[`docker-compose.yml`](../docker-compose.yml).

### Seeded users are missing

Most likely the Postgres volume already existed, so the seed scripts did not re-run. Recreate it:

```bash
docker compose down -v
docker compose up -d --build
```

### API fails to start with "Missing connection string 'Postgres'" (local run)

You skipped [§ B.2](#b2-configure-the-api). Create
`src/ContactManager.API/appsettings.Development.json` from the `.example` and fill in the
connection string and JWT secret.

### Frontend cannot reach the API in local development

The SPA points wherever [`web/src/environments/environment.ts`](../web/src/environments/environment.ts)
says. It ships pointing at the Docker API (`http://localhost:8085/api/v1`). If you run the API
locally with `dotnet run`, set it to `http://localhost:5050/api/v1` (see [§ B.4](#b4-point-the-frontend-at-an-api)).
