-- =============================================================================
-- Seed-Script (PostgreSQL): Light-Quizzes + Tuepfeln-Quizzes + Demo-Gruppe
-- =============================================================================
-- Voraussetzung: Chemicals (1-9), Reactions (1-21), Observations (1-7),
--                Methods (1-3) muessen bereits existieren (via Excel-Import).
-- User Lehrer1 ('11111111-...') + Schueler1 ('44444444-...') werden vom
-- DatabaseSeeder im Development-Mode angelegt.
-- =============================================================================

DO $$
DECLARE
    seed_user_id    uuid    := '11111111-1111-1111-1111-111111111111';  -- Lehrer1
    seed_group_name varchar := 'Demo-Klasse';
    seed_group_id integer := (SELECT "GroupId"
                              FROM "Groups"
                              WHERE "Name" = 'Demo-Klasse');
BEGIN
    -- ── Idempotenz: vorhandene Quiz-Seeds wegräumen (Reihenfolge wegen FKs) ──
    DELETE FROM "GroupQuiz" WHERE "QuizId" IN (1, 2, 3, 4, 5) OR "GroupId" = seed_group_id;
    DELETE FROM "GroupUser" WHERE "GroupId" = seed_group_id;

    DELETE
    FROM "StChemicalResults"
    WHERE "ResultId" IN (SELECT "ResultId"
                         FROM "StResults"
                         WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
                            OR "AttemptId" IN (SELECT "AttemptId" FROM "QuizAttempts" WHERE "QuizId" IN (1, 2, 3, 4, 5))
        );
    DELETE
    FROM "StResults"
    WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
       OR "AttemptId" IN (SELECT "AttemptId" FROM "QuizAttempts" WHERE "QuizId" IN (1, 2, 3, 4, 5));
    DELETE
    FROM "StlResults"
    WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
       OR "AttemptId" IN (SELECT "AttemptId" FROM "QuizAttempts" WHERE "QuizId" IN (1, 2, 3, 4, 5));

    DELETE FROM "QuizAttempts" WHERE "QuizId" IN (1, 2, 3, 4, 5);
    DELETE FROM "StAvailableMethods" WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    DELETE FROM "StAvailableChemicals" WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    DELETE FROM "StlAvailableReactions" WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    DELETE FROM "StlQuestions" WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    DELETE FROM "StQuestions" WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    DELETE FROM "QuizQuestions" WHERE "QuizId" IN (1, 2, 3, 4, 5);
    DELETE FROM "Questions" WHERE "QuestionId" IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    DELETE FROM "Quizzes" WHERE "QuizId" IN (1, 2, 3, 4, 5);
    IF seed_group_id IS NOT NULL THEN
        DELETE FROM "Groups" WHERE "GroupId" = seed_group_id;
        seed_group_id := NULL;
    END IF;

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
    ON CONFLICT ("Name") DO UPDATE SET "HexValue" = EXCLUDED."HexValue";

    -- ============================================================================
    -- LIGHT QUIZZES  (QuestionType = 1 = SpotTestLight)
    -- ============================================================================

    INSERT INTO "Quizzes" ("QuizId", "Name", "CreatedBy")
    VALUES
        (1, 'Niederschlaege erkennen', seed_user_id),
        (2, 'Beobachtungen zuordnen',  seed_user_id);

    -- Quiz 1: Niederschlaege erkennen (3 Fragen)
    INSERT INTO "Questions" ("QuestionId", "Type", "Title", "Description", "CreatedBy")
    VALUES
        (1, 1, 'Light Q1 - gelber Niederschlag Pb(NO3)2', 'Was fuehrt zu einem gelben Niederschlag mit Blei(II)nitrat?', seed_user_id),
        (2, 1, 'Light Q2 - orangebraune Faerbung FeCl3',  'Welche Reaktion zeigt orangebraune Faerbung mit Eisen(III)chlorid?', seed_user_id),
        (3, 1, 'Light Q3 - gelber Niederschlag AgNO3',    'Was erzeugt gelben Niederschlag mit Silber(I)nitrat?', seed_user_id);

    -- Quiz 2: Beobachtungen zuordnen (2 Fragen)
    INSERT INTO "Questions" ("QuestionId", "Type", "Title", "Description", "CreatedBy")
    VALUES
        (4, 1, 'Light Q4 - brauner Niederschlag AgNO3',  'Welche Reaktion verursacht braunen Niederschlag mit Silber(I)nitrat?', seed_user_id),
        (5, 1, 'Light Q5 - brauner Niederschlag NaCO3',  'Was erzeugt braunen Niederschlag mit Natriumcarbonat?', seed_user_id);

    -- StlQuestions (korrekte Reaktion + angezeigter Edukt pro Light-Frage)
    INSERT INTO "StlQuestions" ("QuestionId", "ReactionId", "ShownEductId")
    VALUES
        (1, 7,  2),   -- Q1: korrekt R7 (Pb+KI -> PbI2, gelb),   gezeigt: Pb(NO3)2=2
        (2, 2,  1),   -- Q2: korrekt R2 (Fe+KI -> I2, orange),   gezeigt: FeCl3=1
        (3, 13, 5),   -- Q3: korrekt R13 (KI+Ag -> AgI, gelb),   gezeigt: AgNO3=5
        (4, 20, 5),   -- Q4: korrekt R20 (Ag+Ba(OH)2 -> AgOH, braun), gezeigt: AgNO3=5
        (5, 3,  4);   -- Q5: korrekt R3 (Fe+NaCO3 -> Fe2(CO3)3, braun), gezeigt: NaCO3=4

    INSERT INTO "QuizQuestions" ("QuizId", "QuestionId", "Order")
    VALUES
        (1, 1, 1), (1, 2, 2), (1, 3, 3),
        (2, 4, 1), (2, 5, 2);

    -- StlAvailableReactions (Antwortoptionen pro Frage, je 4 Stueck)
    INSERT INTO "StlAvailableReactions" ("QuestionId", "ReactionId")
    VALUES
        (1, 7),  (1, 8),  (1, 10), (1, 11),
        (2, 2),  (2, 1),  (2, 3),  (2, 4),
        (3, 13), (3, 4),  (3, 16), (3, 19),
        (4, 20), (4, 4),  (4, 13), (4, 19),
        (5, 3),  (5, 8),  (5, 16), (5, 17);

    -- ============================================================================
    -- SPOTTEST / TUEPFELN QUIZZES  (QuestionType = 0 = SpotTest)
    -- ============================================================================

    INSERT INTO "Quizzes" ("QuizId", "Name", "CreatedBy")
    VALUES
        (3, 'Grundlagen',     seed_user_id),
        (4, 'Fortgeschritten', seed_user_id);

    INSERT INTO "Questions" ("QuestionId", "Type", "Title", "Description", "CreatedBy")
    VALUES
        (6, 0, 'Tuepfeln Q6 - drei Unbekannte',  'Bestimme die drei unbekannten Edukte. Du darfst mischen, pH-Papier und Flammenfaerbung verwenden.', seed_user_id),
        (7, 0, 'Tuepfeln Q7 - zwei Unbekannte',  'Identifiziere diese zwei Edukte anhand ihrer Reaktionen und Eigenschaften.', seed_user_id),
        (8, 0, 'Tuepfeln Q8 - vier Unbekannte',  'Vier unbekannte Edukte - nutze alle verfuegbaren Hilfsmittel.', seed_user_id);

    INSERT INTO "StQuestions" ("QuestionId") VALUES (6), (7), (8);

    INSERT INTO "QuizQuestions" ("QuizId", "QuestionId", "Order")
    VALUES
        (3, 6, 1), (3, 7, 2),
        (4, 8, 1);

    -- StAvailableChemicals: Edukte + Zusatzstoffe pro Frage (Order 0-basiert)
    INSERT INTO "StAvailableChemicals" ("QuestionId", "ChemicalId", "Order")
    VALUES
        (6, 1, 0), (6, 3, 1), (6, 5, 2), (6, 8, 3), (6, 9, 4),
        (7, 2, 0), (7, 4, 1), (7, 8, 2), (7, 9, 3),
        (8, 2, 0), (8, 3, 1), (8, 5, 2), (8, 6, 3), (8, 8, 4), (8, 9, 5);

    -- StAvailableMethods: pH-Papier=1, Flammenfaerbung=2
    INSERT INTO "StAvailableMethods" ("QuestionId", "MethodId")
    VALUES
        (6, 1), (6, 2),
        (7, 1), (7, 2),
        (8, 1), (8, 2);

    -- ============================================================================
    -- MIXED QUIZ (1 Light- + 1 Tuepfeln-Frage im selben Quiz)
    -- ============================================================================

    INSERT INTO "Quizzes" ("QuizId", "Name", "CreatedBy")
    VALUES
        (5, 'Gemischt - Light und Tuepfeln', seed_user_id);

    INSERT INTO "Questions" ("QuestionId", "Type", "Title", "Description", "CreatedBy")
    VALUES
        (9,  1, 'Mixed Q9 - gelber Niederschlag AgNO3', 'Welche Reaktion erzeugt gelben Niederschlag mit Silber(I)nitrat?', seed_user_id),
        (10, 0, 'Mixed Q10 - zwei Unbekannte',          'Identifiziere die zwei unbekannten Edukte.', seed_user_id);

    -- Q9: korrekt R13 (KI+Ag -> AgI, gelb), gezeigt: AgNO3=5
    INSERT INTO "StlQuestions" ("QuestionId", "ReactionId", "ShownEductId")
    VALUES
        (9, 13, 5);

    INSERT INTO "StQuestions" ("QuestionId") VALUES (10);

    INSERT INTO "QuizQuestions" ("QuizId", "QuestionId", "Order")
    VALUES
        (5, 9,  1),
        (5, 10, 2);

    INSERT INTO "StlAvailableReactions" ("QuestionId", "ReactionId")
    VALUES
        (9, 13), (9, 4), (9, 16), (9, 19);

    INSERT INTO "StAvailableChemicals" ("QuestionId", "ChemicalId", "Order")
    VALUES
        (10, 5, 0), (10, 3, 1), (10, 8, 2), (10, 9, 3);

    INSERT INTO "StAvailableMethods" ("QuestionId", "MethodId")
    VALUES
        (10, 1), (10, 2);

    -- ============================================================================
    -- GROUP: Demo-Klasse mit Lehrer1 + Schueler1, zugewiesen Quiz 5
    -- ============================================================================

    INSERT INTO "Groups" ("Name", "Description") VALUES
        (seed_group_name, 'Seed-Gruppe mit Lehrer1 und Schueler1, gekoppelt an das Mixed-Quiz.')
    RETURNING "GroupId" INTO seed_group_id;

    INSERT INTO "GroupUser" ("GroupId", "UserId")
    VALUES
        (seed_group_id, '11111111-1111-1111-1111-111111111111'::uuid),  -- Lehrer1
        (seed_group_id, '44444444-4444-4444-4444-444444444444'::uuid);  -- Schueler1

    INSERT INTO "GroupQuiz" ("GroupId", "QuizId")
    VALUES
        (seed_group_id, 5);

    -- Identity-Sequences nachziehen, damit folgende Inserts nicht mit Seed-IDs kollidieren
    PERFORM setval(pg_get_serial_sequence('"Quizzes"', 'QuizId'), (SELECT MAX("QuizId") FROM "Quizzes"));
    PERFORM setval(pg_get_serial_sequence('"Questions"', 'QuestionId'), (SELECT MAX("QuestionId") FROM "Questions"));
END $$;
