# C4: Container Diagram

This view shows the main runtime containers and the important internal boundaries that are
relevant for the code review.

```mermaid
flowchart LR
    user[User]

    subgraph frontend[Frontend container]
        ui[Angular SPA]
    end

    subgraph backend[Backend container]
        api[ASP.NET Core API]
        app[Application layer]
        domain[Domain layer]
        data[Infrastructure.Data]
        identity[Infrastructure.Identity]
    end

    db[(PostgreSQL)]

    user -->|Browser| ui
    ui -->|HTTP JSON + JWT| api
    api --> app
    app --> domain
    api --> identity
    app --> data
    data --> db
    identity -->|JWT generation / password hashing| api
```

## Notes

- `Domain` contains entities and invariants only.
- `Application` contains use cases, validation, DTOs, and interfaces.
- `Infrastructure.Data` implements repositories with raw SQL through `Npgsql`.
- `Infrastructure.Identity` implements password hashing and token generation.
- The API composes everything and exposes REST endpoints.
