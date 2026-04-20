-- =============================================================================
-- Seed-Script: Additiv-Reaktionen + Light-Quizzes + Tuepfeln-Quizzes
-- =============================================================================
-- Voraussetzung: Chemicals (1-9), Reactions (1-21), Observations (1-7),
--                Methods (1-3) muessen bereits existieren (via Excel-Import).
-- =============================================================================

BEGIN;

-- ── Lehrer1 als Ersteller verwenden ──────────────────────────────────────────
-- Voraussetzung: User muss existieren (wird vom DatabaseSeeder beim ersten
-- App-Start im Development-Mode angelegt).
DECLARE @SeedUserID UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111'; -- Lehrer1

-- Demo-Group wird ueber den Namen identifiziert (GroupID dynamisch),
-- damit eine evtl. bereits manuell angelegte Group mit ID 1 nicht weggeraeumt wird.
DECLARE @SeedGroupName NVARCHAR(64) = N'Demo-Klasse';
DECLARE @SeedGroupID INT = (SELECT GroupID FROM Groups WHERE Name = @SeedGroupName);

-- ── Idempotenz: vorhandene Quiz-Seeds wegräumen (Reihenfolge wegen FKs) ─────
-- Group-Joins zuerst, sonst blockieren sie Quiz/Group-Deletes (FK Restrict).
DELETE FROM GroupQuiz             WHERE QuizID IN (1,2,3,4,5) OR GroupID = @SeedGroupID;
DELETE FROM GroupUser             WHERE GroupID = @SeedGroupID;
-- Results + Attempts zuerst löschen; sonst blockieren sie Questions/Quizzes.
DELETE FROM STChemicalResults
    WHERE ResultID IN (
        SELECT ResultID FROM STResults
        WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10)
           OR AttemptID IN (SELECT AttemptID FROM QuizAttempts WHERE QuizID IN (1,2,3,4,5))
    );
DELETE FROM STResults
    WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10)
       OR AttemptID IN (SELECT AttemptID FROM QuizAttempts WHERE QuizID IN (1,2,3,4,5));
DELETE FROM STLResults
    WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10)
       OR AttemptID IN (SELECT AttemptID FROM QuizAttempts WHERE QuizID IN (1,2,3,4,5));
DELETE FROM QuizAttempts          WHERE QuizID IN (1,2,3,4,5);
DELETE FROM STAvailableMethods    WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM STAvailableChemicals  WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM STLAvailableReactions WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM STLQuestions          WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM STQuestions           WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM QuizQuestions         WHERE QuizID IN (1,2,3,4,5);
DELETE FROM Questions             WHERE QuestionID IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM Quizzes               WHERE QuizID IN (1,2,3,4,5);
DELETE FROM Groups                WHERE GroupID = @SeedGroupID;

-- ============================================================================
-- HINWEIS: Additiv-Reaktionen (Edukt+NaOH/HCl) kommen aus dem Excel-Import.
-- Falls sie noch fehlen: Excel neu importieren (Import-Bug wurde gefixt).
-- ============================================================================


-- ============================================================================
-- LIGHT QUIZZES  (QuestionType = 1 = SpotTestLight)
-- ============================================================================

-- ── Quizzes ─────────────────────────────────────────────────────────────────
INSERT INTO "Quizzes" ("QuizID", "Name", "CreatedBy") VALUES
    (1, 'Niederschlaege erkennen', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    (2, 'Beobachtungen zuordnen', '9c9c2138-f945-41fa-823e-f3bd286c0fa1');

-- ── Questions (Light) ───────────────────────────────────────────────────────
-- Hinweis: Korrekte Reaktion + angezeigter Edukt liegen jetzt in STLQuestions

-- Quiz 1: Niederschlaege erkennen (3 Fragen)
INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
    (1, 1, 'Light Q1 - gelber Niederschlag Pb(NO3)2', 'Was fuehrt zu einem gelben Niederschlag mit Blei(II)nitrat?', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    (2, 1, 'Light Q2 - orangebraune Faerbung FeCl3', 'Welche Reaktion zeigt orangebraune Faerbung mit Eisen(III)chlorid?', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    (3, 1, 'Light Q3 - gelber Niederschlag AgNO3', 'Was erzeugt gelben Niederschlag mit Silber(I)nitrat?', '9c9c2138-f945-41fa-823e-f3bd286c0fa1');

-- Quiz 2: Beobachtungen zuordnen (2 Fragen)
INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
    (4, 1, 'Light Q4 - brauner Niederschlag AgNO3', 'Welche Reaktion verursacht braunen Niederschlag mit Silber(I)nitrat?', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    (5, 1, 'Light Q5 - brauner Niederschlag NaCO3', 'Was erzeugt braunen Niederschlag mit Natriumcarbonat?', '9c9c2138-f945-41fa-823e-f3bd286c0fa1');

-- ── STLQuestions (korrekte Reaktion + angezeigter Edukt pro Light-Frage) ────
-- ReactionID = FK auf Reactions (die korrekte Antwort)
-- ShownEductID = FK auf Chemicals (der im Aufgabentext genannte Edukt)
INSERT INTO "STLQuestions" ("QuestionID", "ReactionID", "ShownEductID") VALUES
    -- Q1: korrekt R7 (Pb+KI -> PbI2, gelb), gezeigt: Pb(NO3)2=2
    (1, 7, 2),
    -- Q2: korrekt R2 (Fe+KI -> I2, orange), gezeigt: FeCl3=1
    (2, 2, 1),
    -- Q3: korrekt R13 (KI+Ag -> AgI, gelb), gezeigt: AgNO3=5
    (3, 13, 5),
    -- Q4: korrekt R20 (Ag+Ba(OH)2 -> AgOH, braun), gezeigt: AgNO3=5
    (4, 20, 5),
    -- Q5: korrekt R3 (Fe+NaCO3 -> Fe2(CO3)3, braun), gezeigt: NaCO3=4
    (5, 3, 4);

-- ── QuizQuestions (Zuordnung Quiz <-> Frage) ────────────────────────────────
INSERT INTO "QuizQuestions" ("QuizID", "QuestionID", "Order") VALUES
    -- Quiz 1
    (1, 1, 1),
    (1, 2, 2),
    (1, 3, 3),
    -- Quiz 2
    (2, 4, 1),
    (2, 5, 2);

-- ── STLAvailableReactions (Antwortoptionen pro Frage, je 4 Stueck) ──────────
INSERT INTO "STLAvailableReactions" ("QuestionID", "ReactionID") VALUES
    -- Q1: Pb(NO3)2 + gelber Niederschlag
    --   R7  = Pb+KI -> PbI2 (gelb)        <- korrekt
    --   R8  = Pb+NaCO3 -> PbCO3 (weiss)
    --   R10 = Pb+SrCl2 -> PbCl2 (weiss)
    --   R11 = Pb+Ba(OH)2 -> Pb(OH)2 (weiss)
    (1, 7), (1, 8), (1, 10), (1, 11),

    -- Q2: FeCl3 + orangebraune Faerbung
    --   R2 = Fe+KI -> I2 (orange)          <- korrekt
    --   R1 = Fe+Pb -> PbCl2 (weiss)
    --   R3 = Fe+NaCO3 -> Fe2(CO3)3 (braun)
    --   R4 = Fe+Ag -> AgCl (weiss)
    (2, 2), (2, 1), (2, 3), (2, 4),

    -- Q3: AgNO3 + gelber Niederschlag
    --   R13 = KI+Ag -> AgI (gelb)          <- korrekt
    --   R4  = Fe+Ag -> AgCl (weiss)
    --   R16 = NaCO3+Ag -> Ag2CO3 (weiss)
    --   R19 = Ag+SrCl2 -> AgCl (weiss)
    (3, 13), (3, 4), (3, 16), (3, 19),

    -- Q4: AgNO3 + brauner Niederschlag
    --   R20 = Ag+Ba(OH)2 -> AgOH (braun)   <- korrekt
    --   R4  = Fe+Ag -> AgCl (weiss)
    --   R13 = KI+Ag -> AgI (gelb)
    --   R19 = Ag+SrCl2 -> AgCl (weiss)
    (4, 20), (4, 4), (4, 13), (4, 19),

    -- Q5: NaCO3 + brauner Niederschlag
    --   R3  = Fe+NaCO3 -> Fe2(CO3)3 (braun) <- korrekt
    --   R8  = Pb+NaCO3 -> PbCO3 (weiss)
    --   R16 = NaCO3+Ag -> Ag2CO3 (weiss)
    --   R17 = NaCO3+SrCl2 -> SrCO3 (weiss)
    (5, 3), (5, 8), (5, 16), (5, 17);


-- ============================================================================
-- SPOTTEST / TUEPFELN QUIZZES  (QuestionType = 2 = Tuepfeln)
-- ============================================================================

-- ── Quizzes ─────────────────────────────────────────────────────────────────
INSERT INTO "Quizzes" ("QuizID", "Name", "CreatedBy") VALUES
    (3, 'Grundlagen', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    (4, 'Fortgeschritten', '9c9c2138-f945-41fa-823e-f3bd286c0fa1');

-- ── Questions (SpotTest / Tuepfeln) ─────────────────────────────────────────
INSERT INTO "Questions" ("QuestionID", "Type", "Title", "Description", "CreatedBy") VALUES
    -- Quiz 3: Grundlagen (2 Aufgaben)
    (6, 0, 'Tuepfeln Q6 - drei Unbekannte', 'Bestimme die drei unbekannten Edukte. Du darfst mischen, pH-Papier und Flammenfaerbung verwenden.', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    (7, 0, 'Tuepfeln Q7 - zwei Unbekannte', 'Identifiziere diese zwei Edukte anhand ihrer Reaktionen und Eigenschaften.', '9c9c2138-f945-41fa-823e-f3bd286c0fa1'),
    -- Quiz 4: Fortgeschritten (1 Aufgabe)
    (8, 0, 'Tuepfeln Q8 - vier Unbekannte', 'Vier unbekannte Edukte - nutze alle verfuegbaren Hilfsmittel.', '9c9c2138-f945-41fa-823e-f3bd286c0fa1');

-- ── STQuestions (Parent-Row pro SpotTest-Frage, FK von Available* benoetigt) ─
INSERT INTO "STQuestions" ("QuestionID") VALUES (6), (7), (8);

-- ── QuizQuestions ───────────────────────────────────────────────────────────
INSERT INTO "QuizQuestions" ("QuizID", "QuestionID", "Order") VALUES
    -- Quiz 3: Grundlagen
    (3, 6, 1),
    (3, 7, 2),
    -- Quiz 4: Fortgeschritten
    (4, 8, 1);

-- ── STAvailableChemicals (Unbekannte Edukte + Zusatzstoffe zum Mischen) ────
-- Edukte (Type=0) werden als "Unbekannte" angezeigt
-- Zusatzstoffe (Type=1) werden als Hilfsmittel angezeigt
-- Order: Reihenfolge der Anzeige (0-basiert)
INSERT INTO "STAvailableChemicals" ("QuestionID", "ChemicalID", "Order") VALUES
    -- Q6: Unbekannte = FeCl3(1), KI(3), AgNO3(5) + Zusatzstoffe NaOH(8), HCl(9)
    (6, 1, 0), (6, 3, 1), (6, 5, 2), (6, 8, 3), (6, 9, 4),
    -- Q7: Unbekannte = Pb(NO3)2(2), NaCO3(4) + Zusatzstoffe NaOH(8), HCl(9)
    (7, 2, 0), (7, 4, 1), (7, 8, 2), (7, 9, 3),
    -- Q8: Unbekannte = Pb(NO3)2(2), KI(3), AgNO3(5), SrCl2(6) + Zusatzstoffe NaOH(8), HCl(9)
    (8, 2, 0), (8, 3, 1), (8, 5, 2), (8, 6, 3), (8, 8, 4), (8, 9, 5);

-- ── STAvailableMethods (verfuegbare Analysemethoden pro Frage) ──────────────
-- MethodIDs: ph-Papier=1, Flammenfaerbung=2 (Eigenfarbe ist keine Method mehr)
INSERT INTO "STAvailableMethods" ("QuestionID", "MethodID") VALUES
    -- Q6: ph-Papier(1) + Flammenfaerbung(2)
    (6, 1), (6, 2),
    -- Q7: ph-Papier(1) + Flammenfaerbung(2)
    (7, 1), (7, 2),
    -- Q8: ph-Papier(1) + Flammenfaerbung(2)
    (8, 1), (8, 2);


-- ============================================================================
-- MIXED QUIZ  (1 Light- + 1 Tuepfeln-Frage im selben Quiz)
-- ============================================================================

-- ── Quiz 5 ──────────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Quizzes ON;

INSERT INTO Quizzes (QuizID, Name, CreatedBy) VALUES
    (5, 'Gemischt - Light und Tuepfeln', @SeedUserID);

SET IDENTITY_INSERT Quizzes OFF;

-- ── Questions (eine je Type) ────────────────────────────────────────────────
SET IDENTITY_INSERT Questions ON;

INSERT INTO Questions (QuestionID, Type, Title, Description, CreatedBy) VALUES
    (9,  1, 'Mixed Q9 - gelber Niederschlag AgNO3', 'Welche Reaktion erzeugt gelben Niederschlag mit Silber(I)nitrat?', @SeedUserID),
    (10, 0, 'Mixed Q10 - zwei Unbekannte',          'Identifiziere die zwei unbekannten Edukte.', @SeedUserID);

SET IDENTITY_INSERT Questions OFF;

-- Light-Parent fuer Q9
-- Q9: korrekt R13 (KI+Ag -> AgI, gelb), gezeigt: AgNO3=5
INSERT INTO STLQuestions (QuestionID, ReactionID, ShownEductID) VALUES
    (9, 13, 5);

-- SpotTest-Parent fuer Q10
INSERT INTO STQuestions (QuestionID) VALUES (10);

INSERT INTO QuizQuestions (QuizID, QuestionID, [Order]) VALUES
    (5, 9,  1),
    (5, 10, 2);

-- Antwortoptionen fuer Q9 (analog Q3)
INSERT INTO STLAvailableReactions (QuestionID, ReactionID) VALUES
    (9, 13), (9, 4), (9, 16), (9, 19);

-- Q10: Unbekannte = AgNO3(5), KI(3) + Zusatzstoffe NaOH(8), HCl(9)
INSERT INTO STAvailableChemicals (QuestionID, ChemicalID, [Order]) VALUES
    (10, 5, 0), (10, 3, 1), (10, 8, 2), (10, 9, 3);

-- Q10: ph-Papier + Flammenfaerbung
INSERT INTO STAvailableMethods (QuestionID, MethodID) VALUES
    (10, 1), (10, 2);


-- ============================================================================
-- GROUP: Demo-Klasse mit Lehrer1 + Schueler1, zugewiesen Quiz 5
-- ============================================================================

INSERT INTO Groups (Name, Description) VALUES
    (@SeedGroupName, N'Seed-Gruppe mit Lehrer1 und Schueler1, gekoppelt an das Mixed-Quiz.');

SET @SeedGroupID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO GroupUser (GroupID, UserID) VALUES
    (@SeedGroupID, '11111111-1111-1111-1111-111111111111'),  -- Lehrer1
    (@SeedGroupID, '44444444-4444-4444-4444-444444444444');  -- Schueler1

INSERT INTO GroupQuiz (GroupID, QuizID) VALUES
    (@SeedGroupID, 5);

COMMIT TRANSACTION;

-- Verification
SELECT 'Reactions' AS [Table], COUNT(*) AS [Count] FROM Reactions
UNION ALL SELECT 'Quizzes', COUNT(*) FROM Quizzes
UNION ALL SELECT 'Questions', COUNT(*) FROM Questions
UNION ALL SELECT 'STLQuestions', COUNT(*) FROM STLQuestions
UNION ALL SELECT 'STQuestions', COUNT(*) FROM STQuestions
UNION ALL SELECT 'QuizQuestions', COUNT(*) FROM QuizQuestions
UNION ALL SELECT 'STLAvailableReactions', COUNT(*) FROM STLAvailableReactions
UNION ALL SELECT 'STAvailableChemicals', COUNT(*) FROM STAvailableChemicals
UNION ALL SELECT 'STAvailableMethods', COUNT(*) FROM STAvailableMethods
UNION ALL SELECT 'Groups', COUNT(*) FROM Groups
UNION ALL SELECT 'GroupUser', COUNT(*) FROM GroupUser
UNION ALL SELECT 'GroupQuiz', COUNT(*) FROM GroupQuiz;
