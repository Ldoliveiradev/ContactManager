-- Demo seed data (the brief requires seeded data / credentials for the demo).
--
-- Demo logins:  demo / Demo123!   and   sales2 / Demo123!
--
-- Two users with the SAME password ("Demo123!") so the demo can show that contacts are
-- private per user. The password_hash is identical for both because PBKDF2 verification
-- uses the salt embedded in the hash, so the same stored hash validates "Demo123!" for
-- either user. The hash was produced by
-- ContactManager.Infrastructure.Security.Pbkdf2PasswordHasher (format
-- "iterations.saltBase64.keyBase64").

INSERT INTO users (id, username, password_hash)
VALUES
    ('4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'demo',
     '100000.bv0wMexe7CQRY1URyMmNQw==.KUBwEsTLOzSNIejt7V2AEgVOEFHTbIjw7dujeeJkUQ8='),
    ('9487f2c3-c5b3-457d-a540-c50d3f4840e6', 'sales2',
     '100000.bv0wMexe7CQRY1URyMmNQw==.KUBwEsTLOzSNIejt7V2AEgVOEFHTbIjw7dujeeJkUQ8=')
ON CONFLICT (username) DO NOTHING;

-- "demo" owns a larger set so search / sort / pagination are demonstrable.
INSERT INTO contacts (id, user_id, name, email, phone)
VALUES
    ('c23a6f96-6d1e-4cfd-9920-1591a122929a', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Ada Lovelace',      'ada@example.com',      '2025550100'),
    ('2dd87b0d-1042-4b7d-93fc-063d471917a0', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Alan Turing',       'alan@example.com',     '2025550101'),
    ('7d37504f-053f-4eeb-81dd-f3c50781a065', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Grace Hopper',      'grace@example.com',    '2025550102'),
    ('da6e8ed2-e789-45fb-92d7-c0e8175fea2a', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Katherine Johnson', 'katherine@example.com','2025550103'),
    ('26759ad7-57a5-4156-bdba-1cfaf07e6a8a', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Linus Torvalds',    'linus@example.com',    '2025550104'),
    ('af0304b9-ae2e-4d6c-a6da-f9009f0aa735', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Margaret Hamilton', 'margaret@example.com', '2025550105'),
    ('880c1d03-addc-4a1c-a810-e73bcbec320f', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Dennis Ritchie',    'dennis@example.com',   '2025550106'),
    ('d8f6ecb4-665a-4ffd-857c-d91d5238cabd', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Barbara Liskov',    'barbara@example.com',  '2025550107'),
    ('caf3456c-5c9c-4000-abee-6b6b184f3184', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Tim Berners-Lee',   'tim@example.com',      '2025550108'),
    ('f75493cc-c7f4-4ee2-8700-ba253f56feee', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Donald Knuth',      'donald@example.com',   '2025550109'),
    ('8e747f8b-7db7-4856-8515-9f4190c8b452', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Radia Perlman',     'radia@example.com',    '2025550110'),
    ('ac63fc54-ae79-418b-9ac9-279cfbd47eb1', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Ken Thompson',      'ken@example.com',      '2025550111'),
    ('b9e7adae-d444-4655-b8ef-4b65dd36d304', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Hedy Lamarr',       'hedy@example.com',     '2025550112'),
    ('4b2bc0c0-ecef-4398-b3fe-0e7cc9c2bbbe', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'John von Neumann',  'john@example.com',     '2025550113'),
    ('f88c3893-788e-4d8a-8e7b-61497e3bc78b', '4a814ae2-5bc8-48d1-982c-95b6e6d14848', 'Edsger Dijkstra',   'edsger@example.com',   '2025550114'),
    -- "sales2" owns a separate set — demonstrates that each user sees only their own data.
    ('de1a4960-b04c-4c73-b47f-db85abe6132f', '9487f2c3-c5b3-457d-a540-c50d3f4840e6', 'Marie Curie',       'marie@example.com',    '2025550201'),
    ('3bfe8698-1d38-438c-bf13-31ffeaeceec7', '9487f2c3-c5b3-457d-a540-c50d3f4840e6', 'Rosalind Franklin', 'rosalind@example.com', '2025550202'),
    ('e2a346d0-d4ae-407d-a2cb-47f2afa4cfc3', '9487f2c3-c5b3-457d-a540-c50d3f4840e6', 'Nikola Tesla',      'nikola@example.com',   '2025550203')
ON CONFLICT (id) DO NOTHING;
