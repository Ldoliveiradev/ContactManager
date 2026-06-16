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
    ('00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-000000000001', 'Ada Lovelace',      'ada@example.com',      '2025550100'),
    ('00000000-0000-0000-0000-0000000000a2', '00000000-0000-0000-0000-000000000001', 'Alan Turing',       'alan@example.com',     '2025550101'),
    ('00000000-0000-0000-0000-0000000000a3', '00000000-0000-0000-0000-000000000001', 'Grace Hopper',      'grace@example.com',    '2025550102'),
    ('00000000-0000-0000-0000-0000000000a4', '00000000-0000-0000-0000-000000000001', 'Katherine Johnson', 'katherine@example.com','2025550103'),
    ('00000000-0000-0000-0000-0000000000a5', '00000000-0000-0000-0000-000000000001', 'Linus Torvalds',    'linus@example.com',    '2025550104'),
    ('00000000-0000-0000-0000-0000000000a6', '00000000-0000-0000-0000-000000000001', 'Margaret Hamilton', 'margaret@example.com', '2025550105'),
    ('00000000-0000-0000-0000-0000000000a7', '00000000-0000-0000-0000-000000000001', 'Dennis Ritchie',    'dennis@example.com',   '2025550106'),
    ('00000000-0000-0000-0000-0000000000a8', '00000000-0000-0000-0000-000000000001', 'Barbara Liskov',    'barbara@example.com',  '2025550107'),
    ('00000000-0000-0000-0000-0000000000a9', '00000000-0000-0000-0000-000000000001', 'Tim Berners-Lee',   'tim@example.com',      '2025550108'),
    ('00000000-0000-0000-0000-0000000000aa', '00000000-0000-0000-0000-000000000001', 'Donald Knuth',      'donald@example.com',   '2025550109'),
    ('00000000-0000-0000-0000-0000000000ab', '00000000-0000-0000-0000-000000000001', 'Radia Perlman',     'radia@example.com',    '2025550110'),
    ('00000000-0000-0000-0000-0000000000ac', '00000000-0000-0000-0000-000000000001', 'Ken Thompson',      'ken@example.com',      '2025550111'),
    ('00000000-0000-0000-0000-0000000000ad', '00000000-0000-0000-0000-000000000001', 'Hedy Lamarr',       'hedy@example.com',     '2025550112'),
    ('00000000-0000-0000-0000-0000000000ae', '00000000-0000-0000-0000-000000000001', 'John von Neumann',  'john@example.com',     '2025550113'),
    ('00000000-0000-0000-0000-0000000000af', '00000000-0000-0000-0000-000000000001', 'Edsger Dijkstra',   'edsger@example.com',   '2025550114')
ON CONFLICT (id) DO NOTHING;
