# Sequence: Register, Login, Create Contact

This sequence is useful in the interview because it shows the main happy path plus where
security decisions are enforced.

```mermaid
sequenceDiagram
    actor User
    participant UI as Angular SPA
    participant API as ASP.NET Core API
    participant Auth as Auth service
    participant Contact as Contact service
    participant DB as PostgreSQL

    User->>UI: Submit register form
    UI->>API: POST /api/v1/auth/register
    API->>Auth: RegisterAsync(request)
    Auth->>DB: Insert user + account
    DB-->>Auth: Created
    Auth-->>API: Success
    API-->>UI: 201 Created

    User->>UI: Submit login form
    UI->>API: POST /api/v1/auth/login
    API->>Auth: LoginAsync(request)
    Auth->>DB: Load user by username
    DB-->>Auth: User row
    Auth->>Auth: Verify password hash
    Auth-->>API: JWT token
    API-->>UI: 200 OK + token

    User->>UI: Submit new contact
    UI->>API: POST /api/v1/contacts + bearer token
    API->>API: Read user id from JWT
    API->>Contact: CreateAsync(ownerId, request)
    Contact->>DB: Insert contact with account_id = ownerId
    DB-->>Contact: Created
    Contact-->>API: Contact DTO
    API-->>UI: 201 Created
```

## Security point

The owner id used by contact operations comes from the JWT, not from the browser payload.
That is the control that prevents a caller from creating or manipulating data for another
user by tampering with requests.
