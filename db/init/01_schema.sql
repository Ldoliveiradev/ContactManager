-- ContactManager schema
-- accounts table: stores account manager identities (authentication + profile).
-- contacts table: the account manager's private book of business contacts.
-- No ORM is used anywhere; this DDL is applied by the Postgres init container and the
-- Infrastructure layer talks to these tables with hand-written SQL via Npgsql (raw ADO.NET).

CREATE TABLE IF NOT EXISTS accounts (
    id            UUID         PRIMARY KEY,
    username      VARCHAR(100) NOT NULL UNIQUE,
    first_name    VARCHAR(100) NOT NULL,
    last_name     VARCHAR(100) NOT NULL,
    email         VARCHAR(200) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT now(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT now()
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
