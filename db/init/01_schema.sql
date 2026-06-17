-- ContactManager schema
-- users table:    authentication identity (login). An application/auth concern.
-- accounts table: the business identity (account manager profile + book owner).
-- contacts table: the account manager's private book of business contacts.
--
-- A user and their account are 1:1 and SHARE THE SAME id (set together at
-- registration), so accounts.id is also the foreign key back to users.id.
-- This keeps authentication (users) separate from the business domain (accounts)
-- without an extra surrogate key or join column.
--
-- No ORM is used anywhere; this DDL is applied by the Postgres init container and
-- the Infrastructure layer talks to these tables with hand-written SQL via Npgsql.

CREATE TABLE IF NOT EXISTS users (
    id            UUID         PRIMARY KEY,
    username      VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS accounts (
    id         UUID         PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    first_name VARCHAR(100) NOT NULL,
    last_name  VARCHAR(100) NOT NULL,
    email      VARCHAR(200) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS contacts (
    id         UUID         PRIMARY KEY,
    account_id UUID         NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,
    name       VARCHAR(200) NOT NULL,
    email      VARCHAR(200) NOT NULL,
    phone      VARCHAR(50),
    created_at TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ  NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_contacts_account_id ON contacts(account_id);
