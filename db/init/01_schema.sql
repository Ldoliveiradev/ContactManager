-- ContactManager schema
-- Two tables per the exercise brief: one for users (auth), one for app data (contacts).
-- No ORM is used anywhere; this DDL is applied by the Postgres init container and the
-- Infrastructure layer talks to these tables with hand-written SQL via Npgsql (raw ADO.NET).

CREATE TABLE IF NOT EXISTS users (
    id            UUID PRIMARY KEY,
    username      VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS contacts (
    id         UUID PRIMARY KEY,
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name       VARCHAR(200) NOT NULL,
    email      VARCHAR(200) NOT NULL,
    phone      VARCHAR(50),
    created_at TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_contacts_user_id ON contacts(user_id);
