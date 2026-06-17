# Setup From Scratch

This guide is for someone cloning the repository for the first time and wanting the
shortest path to a working demo.

## 1. Clone the repository

```bash
git clone https://github.com/Ldoliveiradev/ContactManager.git
cd ContactManager
```

## 2. Choose how to run it

### Recommended: full Docker setup

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

Demo credentials are in the root [`README.md`](../README.md).

### Alternative: database in Docker, app processes on host

Prerequisites:

- .NET 10 SDK
- Node.js 22+
- Docker

Start only PostgreSQL:

```bash
docker compose up -d postgres
```

Run the API:

```bash
dotnet restore
dotnet run --project src/ContactManager.API
```

Run the frontend:

```bash
cd web
npm install
npm start
```

Then open:

- Frontend: `http://localhost:4200`

## 3. Important first-run note about seeded data

The PostgreSQL container seeds schema and demo data only when the database volume is empty.

If you already ran the project before and want a clean seeded database again:

```bash
docker compose down -v
docker compose up -d --build
```

That removes the existing Postgres volume and recreates the database from the scripts in
[`db/init/`](../db/init/).

## 4. Verify the app works

1. Open the frontend.
2. Sign in with one of the seeded users from the root README.
3. Confirm contacts load.
4. Create, edit, and delete a contact.
5. Open the profile page and verify account details load.

## 5. Run tests

Backend:

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

End-to-end:

```bash
docker compose up -d --build
cd web
npm install
npm run e2e
```

## 6. Common first-time issues

### Port already in use

The project uses these host ports:

- `4300` frontend
- `8085` API
- `5433` PostgreSQL
- `5051` pgAdmin

If one is busy, change the host side of the mapping in [`docker-compose.yml`](../docker-compose.yml).

### Seeded users are missing

Most likely the Postgres volume already existed. Recreate it with:

```bash
docker compose down -v
docker compose up -d --build
```

### Frontend cannot reach the API in local development

Check [`web/src/environments/environment.ts`](../web/src/environments/environment.ts).
It currently points to `http://localhost:8085/api/v1`.

If you run the API outside Docker on a different port, update that value accordingly.
