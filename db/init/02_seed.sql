-- Demo seed data (the brief requires seeded data / credentials for the demo).
--
-- Demo login:  username = "demo"   password = "Demo123!"
--
-- NOTE: password_hash below is a PLACEHOLDER. It must be regenerated to match the
-- hashing scheme implemented in the Application layer (e.g. PBKDF2 / BCrypt) and the
-- exact "Demo123!" plaintext. There is a helper to (re)generate it; see README.
-- Using a fixed UUID for the demo user so seeded contacts can reference it deterministically.

INSERT INTO users (id, username, password_hash)
VALUES ('00000000-0000-0000-0000-000000000001', 'demo', 'REPLACE_WITH_REAL_HASH')
ON CONFLICT (username) DO NOTHING;

INSERT INTO contacts (id, user_id, name, email, phone)
VALUES
    ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-000000000001', 'Ada Lovelace',   'ada@example.com',   '+1-202-555-0100'),
    ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-000000000001', 'Alan Turing',    'alan@example.com',  '+1-202-555-0101'),
    ('00000000-0000-0000-0000-0000000000a3', '00000000-0000-0000-0000-000000000001', 'Grace Hopper',   'grace@example.com', '+1-202-555-0102')
ON CONFLICT (id) DO NOTHING;
