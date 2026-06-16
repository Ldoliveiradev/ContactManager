-- Demo seed data (the brief requires seeded data / credentials for the demo).
--
-- Demo login:  username = "demo"   password = "Demo123!"
--
-- The password_hash is a real PBKDF2 hash of "Demo123!" produced by
-- ContactManager.Infrastructure.Security.Pbkdf2PasswordHasher
-- (format: iterations.saltBase64.keyBase64), so the seeded user can log in immediately.
-- Using a fixed UUID for the demo user so seeded contacts can reference it deterministically.

INSERT INTO users (id, username, password_hash)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'demo',
    '100000.bv0wMexe7CQRY1URyMmNQw==.KUBwEsTLOzSNIejt7V2AEgVOEFHTbIjw7dujeeJkUQ8='
)
ON CONFLICT (username) DO NOTHING;

INSERT INTO contacts (id, user_id, name, email, phone)
VALUES
    ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-000000000001', 'Ada Lovelace',   'ada@example.com',   '+1-202-555-0100'),
    ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-000000000001', 'Alan Turing',    'alan@example.com',  '+1-202-555-0101'),
    ('00000000-0000-0000-0000-0000000000a3', '00000000-0000-0000-0000-000000000001', 'Grace Hopper',   'grace@example.com', '+1-202-555-0102')
ON CONFLICT (id) DO NOTHING;
