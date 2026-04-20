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
    seed_group_id   integer := (SELECT "GroupID" FROM "Groups" WHERE "Name" = 'Demo-Klasse');
BEGIN
    -- ── Idempotenz: vorhandene Quiz-Seeds wegräumen (Reihenfolge wegen FKs) ──
    DELETE FROM "GroupQuiz"             WHERE "QuizID" IN (1,2,3,4,5) OR "GroupID" = seed_group_id;
    DELETE FROM "GroupUser"             WHERE "GroupID" = seed_group_id;

    DELETE FROM "STChemicalResults"
        WHERE "ResultID" IN (
            SELECT "ResultID" FROM "STResults"
            WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10)
               OR "AttemptID" IN (SELECT "AttemptID" FROM "QuizAttempts" WHERE "QuizID" IN (1,2,3,4,5))
        );
    DELETE FROM "STResults"
        WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10)
           OR "AttemptID" IN (SELECT "AttemptID" FROM "QuizAttempts" WHERE "QuizID" IN (1,2,3,4,5));
    DELETE FROM "STLResults"
        WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10)
           OR "AttemptID" IN (SELECT "AttemptID" FROM "QuizAttempts" WHERE "QuizID" IN (1,2,3,4,5));

    DELETE FROM "QuizAttempts"          WHERE "QuizID" IN (1,2,3,4,5);
    DELETE FROM "STAvailableMethods"    WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10);
    DELETE FROM "STAvailableChemicals"  WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10);
    DELETE FROM "STLAvailableReactions" WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10);
    DELETE FROM "STLQuestions"          WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10);
    DELETE FROM "STQuestions"           WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10);
    DELETE FROM "QuizQuestions"         WHERE "QuizID" IN (1,2,3,4,5);
    DELETE FROM "Questions"             WHERE "QuestionID" IN (1,2,3,4,5,6,7,8,9,10);
    DELETE FROM "Quizzes"               WHERE "QuizID" IN (1,2,3,4,5);
    IF seed_group_id IS NOT NULL THEN
        DELETE FROM "Groups" WHERE "GroupID" = seed_group_id;
        seed_group_id := NULL;
    END IF;

    -- ============================================================================
    -- LIGHT QUIZZES  (QuestionType = 1 = SpotTestLight)
    -- ============================================================================

    INSERT INTO "Quizzes" ("QuizID", "Name", "CreatedBy") VALUES
        (1, 'Niederschlaege erkennen', seed_user_id),
        (2, 'Beobachtungen zuordnen',  seed_user_id);

    -- Quiz 1: Niederschlaege erkennen (3 Fragen)
    INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
        (1, 1, 'Light Q1 - gelber Niederschlag Pb(NO3)2', 'Was fuehrt zu einem gelben Niederschlag mit Blei(II)nitrat?', seed_user_id),
        (2, 1, 'Light Q2 - orangebraune Faerbung FeCl3',  'Welche Reaktion zeigt orangebraune Faerbung mit Eisen(III)chlorid?', seed_user_id),
        (3, 1, 'Light Q3 - gelber Niederschlag AgNO3',    'Was erzeugt gelben Niederschlag mit Silber(I)nitrat?', seed_user_id);

    -- Quiz 2: Beobachtungen zuordnen (2 Fragen)
    INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
        (4, 1, 'Light Q4 - brauner Niederschlag AgNO3',  'Welche Reaktion verursacht braunen Niederschlag mit Silber(I)nitrat?', seed_user_id),
        (5, 1, 'Light Q5 - brauner Niederschlag NaCO3',  'Was erzeugt braunen Niederschlag mit Natriumcarbonat?', seed_user_id);

    -- STLQuestions (korrekte Reaktion + angezeigter Edukt pro Light-Frage)
    INSERT INTO "STLQuestions" ("QuestionID", "ReactionID", "ShownEductID") VALUES
        (1, 7,  2),   -- Q1: korrekt R7 (Pb+KI -> PbI2, gelb),   gezeigt: Pb(NO3)2=2
        (2, 2,  1),   -- Q2: korrekt R2 (Fe+KI -> I2, orange),   gezeigt: FeCl3=1
        (3, 13, 5),   -- Q3: korrekt R13 (KI+Ag -> AgI, gelb),   gezeigt: AgNO3=5
        (4, 20, 5),   -- Q4: korrekt R20 (Ag+Ba(OH)2 -> AgOH, braun), gezeigt: AgNO3=5
        (5, 3,  4);   -- Q5: korrekt R3 (Fe+NaCO3 -> Fe2(CO3)3, braun), gezeigt: NaCO3=4

    INSERT INTO "QuizQuestions" ("QuizID", "QuestionID", "Order") VALUES
        (1, 1, 1), (1, 2, 2), (1, 3, 3),
        (2, 4, 1), (2, 5, 2);

    -- STLAvailableReactions (Antwortoptionen pro Frage, je 4 Stueck)
    INSERT INTO "STLAvailableReactions" ("QuestionID", "ReactionID") VALUES
        (1, 7),  (1, 8),  (1, 10), (1, 11),
        (2, 2),  (2, 1),  (2, 3),  (2, 4),
        (3, 13), (3, 4),  (3, 16), (3, 19),
        (4, 20), (4, 4),  (4, 13), (4, 19),
        (5, 3),  (5, 8),  (5, 16), (5, 17);

    -- ============================================================================
    -- SPOTTEST / TUEPFELN QUIZZES  (QuestionType = 0 = SpotTest)
    -- ============================================================================

    INSERT INTO "Quizzes" ("QuizID", "Name", "CreatedBy") VALUES
        (3, 'Grundlagen',     seed_user_id),
        (4, 'Fortgeschritten', seed_user_id);

    INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
        (6, 0, 'Tuepfeln Q6 - drei Unbekannte',  'Bestimme die drei unbekannten Edukte. Du darfst mischen, pH-Papier und Flammenfaerbung verwenden.', seed_user_id),
        (7, 0, 'Tuepfeln Q7 - zwei Unbekannte',  'Identifiziere diese zwei Edukte anhand ihrer Reaktionen und Eigenschaften.', seed_user_id),
        (8, 0, 'Tuepfeln Q8 - vier Unbekannte',  'Vier unbekannte Edukte - nutze alle verfuegbaren Hilfsmittel.', seed_user_id);

    INSERT INTO "STQuestions" ("QuestionID") VALUES (6), (7), (8);

    INSERT INTO "QuizQuestions" ("QuizID", "QuestionID", "Order") VALUES
        (3, 6, 1), (3, 7, 2),
        (4, 8, 1);

    -- STAvailableChemicals: Edukte + Zusatzstoffe pro Frage (Order 0-basiert)
    INSERT INTO "STAvailableChemicals" ("QuestionID", "ChemicalID", "Order") VALUES
        (6, 1, 0), (6, 3, 1), (6, 5, 2), (6, 8, 3), (6, 9, 4),
        (7, 2, 0), (7, 4, 1), (7, 8, 2), (7, 9, 3),
        (8, 2, 0), (8, 3, 1), (8, 5, 2), (8, 6, 3), (8, 8, 4), (8, 9, 5);

    -- STAvailableMethods: pH-Papier=1, Flammenfaerbung=2
    INSERT INTO "STAvailableMethods" ("QuestionID", "MethodID") VALUES
        (6, 1), (6, 2),
        (7, 1), (7, 2),
        (8, 1), (8, 2);

    -- ============================================================================
    -- MIXED QUIZ (1 Light- + 1 Tuepfeln-Frage im selben Quiz)
    -- ============================================================================

    INSERT INTO "Quizzes" ("QuizID", "Name", "CreatedBy") VALUES
        (5, 'Gemischt - Light und Tuepfeln', seed_user_id);

    INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
        (9,  1, 'Mixed Q9 - gelber Niederschlag AgNO3', 'Welche Reaktion erzeugt gelben Niederschlag mit Silber(I)nitrat?', seed_user_id),
        (10, 0, 'Mixed Q10 - zwei Unbekannte',          'Identifiziere die zwei unbekannten Edukte.', seed_user_id);

    -- Q9: korrekt R13 (KI+Ag -> AgI, gelb), gezeigt: AgNO3=5
    INSERT INTO "STLQuestions" ("QuestionID", "ReactionID", "ShownEductID") VALUES
        (9, 13, 5);

    INSERT INTO "STQuestions" ("QuestionID") VALUES (10);

    INSERT INTO "QuizQuestions" ("QuizID", "QuestionID", "Order") VALUES
        (5, 9,  1),
        (5, 10, 2);

    INSERT INTO "STLAvailableReactions" ("QuestionID", "ReactionID") VALUES
        (9, 13), (9, 4), (9, 16), (9, 19);

    INSERT INTO "STAvailableChemicals" ("QuestionID", "ChemicalID", "Order") VALUES
        (10, 5, 0), (10, 3, 1), (10, 8, 2), (10, 9, 3);

    INSERT INTO "STAvailableMethods" ("QuestionID", "MethodID") VALUES
        (10, 1), (10, 2);

    -- ============================================================================
    -- GROUP: Demo-Klasse mit Lehrer1 + Schueler1, zugewiesen Quiz 5
    -- ============================================================================

    INSERT INTO "Groups" ("Name", "Description") VALUES
        (seed_group_name, 'Seed-Gruppe mit Lehrer1 und Schueler1, gekoppelt an das Mixed-Quiz.')
    RETURNING "GroupID" INTO seed_group_id;

    INSERT INTO "GroupUser" ("GroupID", "UserID") VALUES
        (seed_group_id, '11111111-1111-1111-1111-111111111111'::uuid),  -- Lehrer1
        (seed_group_id, '44444444-4444-4444-4444-444444444444'::uuid);  -- Schueler1

    INSERT INTO "GroupQuiz" ("GroupID", "QuizID") VALUES
        (seed_group_id, 5);

    -- Identity-Sequences nachziehen, damit folgende Inserts nicht mit Seed-IDs kollidieren
    PERFORM setval(pg_get_serial_sequence('"Quizzes"',   'QuizID'),     (SELECT MAX("QuizID")     FROM "Quizzes"));
    PERFORM setval(pg_get_serial_sequence('"Questions"', 'QuestionID'), (SELECT MAX("QuestionID") FROM "Questions"));
END $$;
