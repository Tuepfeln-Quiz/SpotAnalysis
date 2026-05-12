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

INSERT INTO "Colors" ("ColorId", "Name", "HexValue")
VALUES (1, 'farblos', '#DDDDDD'),
       (2, 'silbrig', '#C0C0C0'),
       (3, 'orange', '#FF8C00'),
       (4, 'keine', '#666666'),
       (5, 'rot', '#DC143C'),
       (6, 'gelb', '#FFD700')
ON CONFLICT ("ColorId") DO NOTHING;

INSERT INTO "Colors" ("Name", "HexValue")
VALUES
-- Neutral / colorless
('keine', '#666666'),
('nichts', '#666666'),
('farblos', '#DDDDDD'),
('durchsichtig', '#DDDDDD'),
('klar', '#DDDDDD'),
('milchig', '#F5F5DC'),
('trüb', '#C9D6DE'),
('trueb', '#C9D6DE'),
-- White / black / gray
('weiß', '#FFFFFF'),
('weiss', '#FFFFFF'),
('schwarz', '#000000'),
('grau', '#808080'),
('hellgrau', '#D3D3D3'),
('dunkelgrau', '#505050'),
-- Red family
('rot', '#DC143C'),
('hellrot', '#FF6B6B'),
('dunkelrot', '#8B0000'),
('karminrot', '#960018'),
('ziegelrot', '#B22222'),
('weinrot', '#722F37'),
('blutrot', '#8A0303'),
('kirschrot', '#990033'),
('rostrot', '#B7410E'),
('purpurrot', '#800080'),
('magenta', '#FF00FF'),
-- Pink
('rosa', '#FFB6C1'),
('hellrosa', '#FFD1DC'),
('dunkelrosa', '#FF1493'),
('pink', '#FF69B4'),
-- Orange family
('orange', '#FF8C00'),
('hellorange', '#FFB347'),
('dunkelorange', '#CC6600'),
('orangerot', '#FF4500'),
('orangegelb', '#FFA500'),
-- Yellow family
('gelb', '#FFD700'),
('hellgelb', '#FFFACD'),
('dunkelgelb', '#B8860B'),
('goldgelb', '#DAA520'),
('zitronengelb', '#FFF44F'),
('gelbgrün', '#9ACD32'),
('gelbbraun', '#D2B48C'),
-- Green family
('grün', '#228B22'),
('gruen', '#228B22'),
('hellgrün', '#90EE90'),
('dunkelgrün', '#006400'),
('blaugrün', '#008B8B'),
('smaragdgrün', '#50C878'),
('olivgrün', '#6B8E23'),
('olive', '#808000'),
('grasgrün', '#4F7942'),
('giftgrün', '#4CBB17'),
('türkisgrün', '#20B2AA'),
-- Blue family
('blau', '#2563EB'),
('hellblau', '#87CEEB'),
('dunkelblau', '#00008B'),
('fahlblau', '#6497B1'),
('königsblau', '#4169E1'),
('marineblau', '#1F3A93'),
('himmelblau', '#87CEEB'),
('stahlblau', '#4682B4'),
('türkisblau', '#00CED1'),
('türkis', '#40E0D0'),
-- Violet / purple
('violett', '#8B00FF'),
('hellviolett', '#B366FF'),
('dunkelviolett', '#5C0099'),
('blauviolett', '#8A2BE2'),
('rotviolett', '#B01E7A'),
('lila', '#A020F0'),
('fliederfarben', '#C8A2C8'),
-- Brown family
('braun', '#8B4513'),
('hellbraun', '#CD853F'),
('dunkelbraun', '#5D2E0A'),
('schokobraun', '#3E2218'),
('rotbraun', '#8B3A1F'),
('rostbraun', '#B7410E'),
('kupferbraun', '#994D2A'),
('beige', '#F5F5DC'),
-- Metallic
('silbrig', '#C0C0C0'),
('silber', '#C0C0C0'),
('silberweiß', '#E0E0E0'),
('silbergrau', '#A9A9A9'),
('gold', '#FFD700'),
('goldfarben', '#FFD700'),
('kupfer', '#B87333'),
('kupferrot', '#B22222'),
('kupferfarben', '#B87333'),
('bronze', '#CD7F32')
ON CONFLICT ("HexValue") DO UPDATE SET "HexValue" = EXCLUDED."HexValue";

INSERT INTO "Chemicals" ("ChemicalID", "Type", "Name", "Formula", "ColorId")
VALUES (1, 0, 'Silber(I)nitrat', 'AgNO3', 1),
       (2, 0, 'Kalium', 'K', 2),
       (3, 0, 'Eisen(III)chlorid', 'FeCl3', 3),
       (4, 1, 'Salzsäure', 'HCl', 4)
ON CONFLICT ("ChemicalID") DO NOTHING;

INSERT INTO "MethodOutputs" ("ChemicalID", "MethodID", "ColorId")
VALUES (3, 1, 5),
       (3, 2, 6)
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
-- DO-Block + PERFORM, damit ExecuteSqlRawAsync nicht über den SELECT-Return-Wert stolpert.
DO $$
BEGIN
    PERFORM setval('"Methods_MethodID_seq"',           (SELECT COALESCE(MAX("MethodID"),      0) FROM "Methods"));
    PERFORM setval('"Chemicals_ChemicalID_seq"',       (SELECT COALESCE(MAX("ChemicalID"),    0) FROM "Chemicals"));
    PERFORM setval('"Colors_ColorId_seq"', (SELECT COALESCE(MAX("ColorId"), 0) FROM "Colors"));
    PERFORM setval('"Observations_ObservationID_seq"', (SELECT COALESCE(MAX("ObservationID"), 0) FROM "Observations"));
    PERFORM setval('"Reactions_ReactionID_seq"',       (SELECT COALESCE(MAX("ReactionID"),    0) FROM "Reactions"));
END $$;
