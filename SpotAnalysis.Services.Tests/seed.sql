INSERT INTO "Users" ("UserName", "UserId", "PasswordHash", "Roles")
VALUES
    ('Admin1',    '30f7b573-eafe-46ae-a1f0-671e928f7f1d', '$argon2id$v=19$m=4096,t=4,p=4$c7X3MP7qrkah8Gceko9/HQ==$yw+sFjxYiLjExajpvAui5uU6eSWWOng/ruJxBDFNtkE=', ARRAY[0]),
    ('Lehrer1',   '9c9c2138-f945-41fa-823e-f3bd286c0fa1', '$argon2id$v=19$m=4096,t=4,p=4$OCGcnEX5+kGCPvO9KGwPoQ==$AMzbYPw2VRHVZS5i90bEAjGpVoT5w8/ketH9oPXosg0=', ARRAY[1]),
    ('Lehrer2',   '48bb93c8-214f-47f0-910f-9056b19de94a', '$argon2id$v=19$m=4096,t=4,p=4$yJO7SE8h8EeRD5BWsZ3pSg==$QdhAViO1sFFFUelU8q0OD+NZosJEkzPGng8TACDMNlc=', ARRAY[1]),
    ('Schueler1', '2195c82c-0a67-4938-9c88-20c089276da5', '$argon2id$v=19$m=4096,t=4,p=4$LMiVIWcKOEmciCDAiSdtpQ==$zFAeuN0AJ1Onp1W8OM2IhqWkcVG1ZVp40LlHXMJM8Sw=', ARRAY[2]),
    ('Schueler2', 'f01c1e4f-c5e0-4f77-a3b3-f59f8b837553', '$argon2id$v=19$m=4096,t=4,p=4$Tx4c8ODFd0+js/Wfi4N1Uw==$H8mXKG+MXA0AiR2yMVIGf114j+eetQZqjF5KC+H1hgY=', ARRAY[2])
ON CONFLICT ("UserId") DO NOTHING;

INSERT INTO "Colors" ("ColorId", "Name", "HexValue", "IsColorless")
VALUES (1, 'farblos', '#DDDDDD', true),
       (2, 'silbrig', '#C0C0C0', false),
       (3, 'orange', '#FF8C00', false),
       (4, 'keine', '#666666', true),
       (5, 'rot', '#DC143C', false),
       (6, 'gelb', '#FFD700', false)
ON CONFLICT ("ColorId") DO NOTHING;

DO
$$
    BEGIN
        PERFORM setval('"Colors_ColorId_seq"', (SELECT COALESCE(MAX("ColorId"), 0) FROM "Colors"));
    END
$$;

INSERT INTO "Colors" ("Name", "HexValue", "IsColorless")
VALUES
-- Neutral / colorless
('keine', '#666666', true),
('nichts', '#666666', true),
('farblos', '#DDDDDD', true),
('durchsichtig', '#DDDDDD', true),
('klar', '#DDDDDD', true),
('milchig', '#F5F5DC', false),
('trüb', '#C9D6DE', false),
('trueb', '#C9D6DE', false),
-- White / black / gray
('weiß', '#FFFFFF', false),
('weiss', '#FFFFFF', false),
('schwarz', '#000000', false),
('grau', '#808080', false),
('hellgrau', '#D3D3D3', false),
('dunkelgrau', '#505050', false),
-- Red family
('rot', '#DC143C', false),
('hellrot', '#FF6B6B', false),
('dunkelrot', '#8B0000', false),
('karminrot', '#960018', false),
('ziegelrot', '#B22222', false),
('weinrot', '#722F37', false),
('blutrot', '#8A0303', false),
('kirschrot', '#990033', false),
('rostrot', '#B7410E', false),
('purpurrot', '#800080', false),
('magenta', '#FF00FF', false),
-- Pink
('rosa', '#FFB6C1', false),
('hellrosa', '#FFD1DC', false),
('dunkelrosa', '#FF1493', false),
('pink', '#FF69B4', false),
-- Orange family
('orange', '#FF8C00', false),
('hellorange', '#FFB347', false),
('dunkelorange', '#CC6600', false),
('orangerot', '#FF4500', false),
('orangegelb', '#FFA500', false),
-- Yellow family
('gelb', '#FFD700', false),
('hellgelb', '#FFFACD', false),
('dunkelgelb', '#B8860B', false),
('goldgelb', '#DAA520', false),
('zitronengelb', '#FFF44F', false),
('gelbgrün', '#9ACD32', false),
('gelbbraun', '#D2B48C', false),
-- Green family
('grün', '#228B22', false),
('gruen', '#228B22', false),
('hellgrün', '#90EE90', false),
('dunkelgrün', '#006400', false),
('blaugrün', '#008B8B', false),
('smaragdgrün', '#50C878', false),
('olivgrün', '#6B8E23', false),
('olive', '#808000', false),
('grasgrün', '#4F7942', false),
('giftgrün', '#4CBB17', false),
('türkisgrün', '#20B2AA', false),
-- Blue family
('blau', '#2563EB', false),
('hellblau', '#87CEEB', false),
('dunkelblau', '#00008B', false),
('fahlblau', '#6497B1', false),
('königsblau', '#4169E1', false),
('marineblau', '#1F3A93', false),
('himmelblau', '#87CEEB', false),
('stahlblau', '#4682B4', false),
('türkisblau', '#00CED1', false),
('türkis', '#40E0D0', false),
-- Violet / purple
('violett', '#8B00FF', false),
('hellviolett', '#B366FF', false),
('dunkelviolett', '#5C0099', false),
('blauviolett', '#8A2BE2', false),
('rotviolett', '#B01E7A', false),
('lila', '#A020F0', false),
('fliederfarben', '#C8A2C8', false),
-- Brown family
('braun', '#8B4513', false),
('hellbraun', '#CD853F', false),
('dunkelbraun', '#5D2E0A', false),
('schokobraun', '#3E2218', false),
('rotbraun', '#8B3A1F', false),
('rostbraun', '#B7410E', false),
('kupferbraun', '#994D2A', false),
('beige', '#F5F5DC', false),
-- Metallic
('silbrig', '#C0C0C0', false),
('silber', '#C0C0C0', false),
('silberweiß', '#E0E0E0', false),
('silbergrau', '#A9A9A9', false),
('gold', '#FFD700', false),
('goldfarben', '#FFD700', false),
('kupfer', '#B87333', false),
('kupferrot', '#B22222', false),
('kupferfarben', '#B87333', false),
('bronze', '#CD7F32', false)
ON CONFLICT ("Name") DO UPDATE SET "HexValue"    = EXCLUDED."HexValue",
                                   "IsColorless" = EXCLUDED."IsColorless";

INSERT INTO "Chemicals" ("ChemicalId", "Type", "Name", "Formula", "ColorId")
VALUES (1, 0, 'Silber(I)nitrat', 'AgNO3', 1),
       (2, 0, 'Kalium', 'K', 2),
       (3, 0, 'Eisen(III)chlorid', 'FeCl3', 3),
       (4, 1, 'Salzsäure', 'HCl', 4)
ON CONFLICT ("ChemicalId") DO NOTHING;

INSERT INTO "MethodOutputs" ("ChemicalId", "Method", "ColorId")
VALUES (3, 1, 5),
       (3, 2, 6)
ON CONFLICT ("ChemicalId", "Method") DO NOTHING;

INSERT INTO "Observations" ("ObservationId", "Description")
VALUES
    (1, 'Some Observation'),
    (2, 'Weißer Niederschlag')
ON CONFLICT ("ObservationId") DO NOTHING;

-- Hinweis: Seeded-Reactions dürfen nicht (1,2), (1,3) oder (1,4) sein, weil MasterDataService-Tests
-- diese Kombinationen frisch anlegen (CreateReactionAsync_NormalizesChemicalOrder, DeleteReactionAsync_*).
INSERT INTO "Reactions" ("ReactionId", "Chemical1Id", "Chemical2Id", "RelevantProduct", "Formula", "ObservationId")
VALUES
    (1, 2, 3, 'Eisen(II)chlorid', 'K + FeCl3 -> KCl + FeCl2', 1)
ON CONFLICT ("ReactionId") DO NOTHING;

-- Identity-Sequences auf Max(ID) setzen, damit spätere Inserts ohne explizite ID keine Kollisionen werfen.
-- DO-Block + PERFORM, damit ExecuteSqlRawAsync nicht über den SELECT-Return-Wert stolpert.
DO $$
BEGIN
    PERFORM setval('"Chemicals_ChemicalId_seq"', (SELECT COALESCE(MAX("ChemicalId"), 0) FROM "Chemicals"));
    PERFORM setval('"Colors_ColorId_seq"', (SELECT COALESCE(MAX("ColorId"), 0) FROM "Colors"));
    PERFORM setval('"Observations_ObservationId_seq"', (SELECT COALESCE(MAX("ObservationId"), 0) FROM "Observations"));
    PERFORM setval('"Reactions_ReactionId_seq"', (SELECT COALESCE(MAX("ReactionId"), 0) FROM "Reactions"));
END $$;
