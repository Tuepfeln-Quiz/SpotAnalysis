INSERT INTO "Users" ("UserName", "UserID", "PasswordHash", "Roles") VALUES
    ('Admin1',    '30f7b573-eafe-46ae-a1f0-671e928f7f1d', '$argon2id$v=19$m=4096,t=4,p=4$c7X3MP7qrkah8Gceko9/HQ==$yw+sFjxYiLjExajpvAui5uU6eSWWOng/ruJxBDFNtkE=', ARRAY[0]),
    ('Lehrer1',   '9c9c2138-f945-41fa-823e-f3bd286c0fa1', '$argon2id$v=19$m=4096,t=4,p=4$OCGcnEX5+kGCPvO9KGwPoQ==$AMzbYPw2VRHVZS5i90bEAjGpVoT5w8/ketH9oPXosg0=', ARRAY[1]),
    ('Lehrer2',   '48bb93c8-214f-47f0-910f-9056b19de94a', '$argon2id$v=19$m=4096,t=4,p=4$yJO7SE8h8EeRD5BWsZ3pSg==$QdhAViO1sFFFUelU8q0OD+NZosJEkzPGng8TACDMNlc=', ARRAY[1]),
    ('Schueler1', '2195c82c-0a67-4938-9c88-20c089276da5', '$argon2id$v=19$m=4096,t=4,p=4$LMiVIWcKOEmciCDAiSdtpQ==$zFAeuN0AJ1Onp1W8OM2IhqWkcVG1ZVp40LlHXMJM8Sw=', ARRAY[2]),
    ('Schueler2', 'f01c1e4f-c5e0-4f77-a3b3-f59f8b837553', '$argon2id$v=19$m=4096,t=4,p=4$Tx4c8ODFd0+js/Wfi4N1Uw==$H8mXKG+MXA0AiR2yMVIGf114j+eetQZqjF5KC+H1hgY=', ARRAY[2])
ON CONFLICT ("UserID") DO NOTHING;

INSERT INTO "Methods" ("MethodID", "Name") VALUES
    (1, 'ph-Papier'),
    (2, 'Flammenfärbung')
ON CONFLICT ("MethodID") DO NOTHING;

INSERT INTO "Chemicals" ("ChemicalID", "Type", "Name", "Formula", "Color") VALUES
    (1, 0, 'Silber(I)nitrat',   'AgNO3', 'farblos'),
    (2, 0, 'Kalium',             'K',     'silbrig'),
    (3, 0, 'Eisen(III)chlorid', 'FeCl3', 'orange'),
    (4, 1, 'Salzsäure',          'HCl',   'keine')
ON CONFLICT ("ChemicalID") DO NOTHING;

INSERT INTO "MethodOutputs" ("ChemicalID", "MethodID", "Color") VALUES
    (3, 1, 'rot'),
    (3, 2, 'gelb')
ON CONFLICT ("ChemicalID", "MethodID") DO NOTHING;

INSERT INTO "Observations" ("ObservationID", "Description") VALUES
    (1, 'Some Observation'),
    (2, 'Weißer Niederschlag')
ON CONFLICT ("ObservationID") DO NOTHING;

-- Hinweis: Seeded-Reactions dürfen nicht (1,2), (1,3) oder (1,4) sein, weil MasterDataService-Tests
-- diese Kombinationen frisch anlegen (CreateReactionAsync_NormalizesChemicalOrder, DeleteReactionAsync_*).
INSERT INTO "Reactions" ("ReactionID", "Chemical1ID", "Chemical2ID", "RelevantProduct", "Formula", "ObservationID") VALUES
    (1, 2, 3, 'Eisen(II)chlorid', 'K + FeCl3 -> KCl + FeCl2', 1)
ON CONFLICT ("ReactionID") DO NOTHING;

-- Identity-Sequences auf Max(ID) setzen, damit spätere Inserts ohne explizite ID keine Kollisionen werfen.
SELECT setval(pg_get_serial_sequence('"Methods"',     'MethodID'),      COALESCE((SELECT MAX("MethodID")      FROM "Methods"),      1));
SELECT setval(pg_get_serial_sequence('"Chemicals"',   'ChemicalID'),    COALESCE((SELECT MAX("ChemicalID")    FROM "Chemicals"),    1));
SELECT setval(pg_get_serial_sequence('"Observations"','ObservationID'), COALESCE((SELECT MAX("ObservationID") FROM "Observations"), 1));
SELECT setval(pg_get_serial_sequence('"Reactions"',   'ReactionID'),    COALESCE((SELECT MAX("ReactionID")    FROM "Reactions"),    1));
