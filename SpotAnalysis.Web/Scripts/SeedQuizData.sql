-- =============================================================================
-- Seed-Script: Additiv-Reaktionen + Light-Quizzes + Tuepfeln-Quizzes
-- =============================================================================
-- Voraussetzung: Chemicals (1-9), Reactions (1-21), Observations (1-7),
--                Methods (1-3) muessen bereits existieren (via Excel-Import).
-- =============================================================================

BEGIN TRANSACTION;

-- ── Lehrer1 als Ersteller verwenden ──────────────────────────────────────────
-- Voraussetzung: User muss existieren (seed.sql aus Tests oder manuell anlegen)
DECLARE @SeedUserID UNIQUEIDENTIFIER = '9c9c2138-f945-41fa-823e-f3bd286c0fa1'; -- Lehrer1

-- ============================================================================
-- HINWEIS: Additiv-Reaktionen (Edukt+NaOH/HCl) kommen aus dem Excel-Import.
-- Falls sie noch fehlen: Excel neu importieren (Import-Bug wurde gefixt).
-- ============================================================================


-- ============================================================================
-- LIGHT QUIZZES  (QuestionType = 1 = SpotTestLight)
-- ============================================================================

-- ── Quizzes ─────────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Quizzes ON;

INSERT INTO Quizzes (QuizID, Name, CreatedBy) VALUES
    (1, 'Niederschlaege erkennen', @SeedUserID),
    (2, 'Beobachtungen zuordnen', @SeedUserID);

SET IDENTITY_INSERT Quizzes OFF;

-- ── Questions (Light) ───────────────────────────────────────────────────────
SET IDENTITY_INSERT Questions ON;

-- Quiz 1: Niederschlaege erkennen (3 Fragen)
INSERT INTO Questions (QuestionID, Type, Description, CreatedBy) VALUES
    (1, 1, 'Was fuehrt zu einem gelben Niederschlag mit Blei(II)nitrat?', @SeedUserID),
    (2, 1, 'Welche Reaktion zeigt orangebraune Faerbung mit Eisen(III)chlorid?', @SeedUserID),
    (3, 1, 'Was erzeugt gelben Niederschlag mit Silber(I)nitrat?', @SeedUserID);

-- Quiz 2: Beobachtungen zuordnen (2 Fragen)
INSERT INTO Questions (QuestionID, Type, Description, CreatedBy) VALUES
    (4, 1, 'Welche Reaktion verursacht braunen Niederschlag mit Silber(I)nitrat?', @SeedUserID),
    (5, 1, 'Was erzeugt braunen Niederschlag mit Natriumcarbonat?', @SeedUserID);

SET IDENTITY_INSERT Questions OFF;

-- ── QuizQuestions (Zuordnung Quiz <-> Frage) ────────────────────────────────
INSERT INTO QuizQuestions (QuizID, QuestionID, [Order]) VALUES
    -- Quiz 1
    (1, 1, 1),
    (1, 2, 2),
    (1, 3, 3),
    -- Quiz 2
    (2, 4, 1),
    (2, 5, 2);

-- ── STLInput (pro Frage: welches Chemical + welche Observation) ─────────────
-- Das definiert die Aufgabenstellung: "Chemical X zeigt Observation Y"
-- Die korrekte Reaktion wird daraus abgeleitet (Chemical + Observation Match)
INSERT INTO STLInput (QuestionID, ChemicalID, ObservationID) VALUES
    -- Q1: Pb(NO3)2 (2), gelber Niederschlag (5) -> korrekt: R7 (Pb+KI=PbI2)
    (1, 2, 5),
    -- Q2: FeCl3 (1), orangebraune Faerbung (2) -> korrekt: R2 (Fe+KI=I2)
    (2, 1, 2),
    -- Q3: AgNO3 (5), gelber Niederschlag (5) -> korrekt: R13 (KI+Ag=AgI)
    (3, 5, 5),
    -- Q4: AgNO3 (5), brauner Niederschlag (3) -> korrekt: R20 (Ag+Ba(OH)2=AgOH)
    (4, 5, 3),
    -- Q5: NaCO3 (4), brauner Niederschlag (3) -> korrekt: R3 (Fe+NaCO3=Fe2(CO3)3)
    (5, 4, 3);

-- ── STLAvailableReactions (Antwortoptionen pro Frage, je 4 Stueck) ──────────
INSERT INTO STLAvailableReactions (QuestionID, ReactionID) VALUES
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
SET IDENTITY_INSERT Quizzes ON;

INSERT INTO Quizzes (QuizID, Name, CreatedBy) VALUES
    (3, 'Grundlagen', @SeedUserID),
    (4, 'Fortgeschritten', @SeedUserID);

SET IDENTITY_INSERT Quizzes OFF;

-- ── Questions (Tuepfeln) ────────────────────────────────────────────────────
SET IDENTITY_INSERT Questions ON;

INSERT INTO Questions (QuestionID, Type, Description, CreatedBy) VALUES
    -- Quiz 3: Grundlagen (2 Aufgaben)
    (6, 2, 'Bestimme die drei unbekannten Edukte. Du darfst mischen, pH-Papier und Flammenfaerbung verwenden.', @SeedUserID),
    (7, 2, 'Identifiziere diese zwei Edukte anhand ihrer Reaktionen und Eigenschaften.', @SeedUserID),
    -- Quiz 4: Fortgeschritten (1 Aufgabe)
    (8, 2, 'Vier unbekannte Edukte - nutze alle verfuegbaren Hilfsmittel.', @SeedUserID);

SET IDENTITY_INSERT Questions OFF;

-- ── QuizQuestions ───────────────────────────────────────────────────────────
INSERT INTO QuizQuestions (QuizID, QuestionID, [Order]) VALUES
    -- Quiz 3: Grundlagen
    (3, 6, 1),
    (3, 7, 2),
    -- Quiz 4: Fortgeschritten
    (4, 8, 1);

-- ── STAvailableChemicals (Unbekannte Edukte + Zusatzstoffe zum Mischen) ────
-- Edukte (Type=0) werden als "Unbekannte" angezeigt
-- Zusatzstoffe (Type=1) werden als Hilfsmittel angezeigt
INSERT INTO STAvailableChemicals (QuestionID, ChemicalID) VALUES
    -- Q6: Unbekannte = FeCl3(1), KI(3), AgNO3(5) + Zusatzstoffe NaOH(8), HCl(9)
    (6, 1), (6, 3), (6, 5), (6, 8), (6, 9),
    -- Q7: Unbekannte = Pb(NO3)2(2), NaCO3(4) + Zusatzstoffe NaOH(8), HCl(9)
    (7, 2), (7, 4), (7, 8), (7, 9),
    -- Q8: Unbekannte = Pb(NO3)2(2), KI(3), AgNO3(5), SrCl2(6) + Zusatzstoffe NaOH(8), HCl(9)
    (8, 2), (8, 3), (8, 5), (8, 6), (8, 8), (8, 9);

-- ── STAvailableMethods (verfuegbare Analysemethoden pro Frage) ──────────────
INSERT INTO STAvailableMethods (QuestionID, MethodID) VALUES
    -- Q6: pH-Papier(2) + Flammenfaerbung(3)
    (6, 2), (6, 3),
    -- Q7: pH-Papier(2) + Flammenfaerbung(3)
    (7, 2), (7, 3),
    -- Q8: pH-Papier(2) + Flammenfaerbung(3)
    (8, 2), (8, 3);

COMMIT TRANSACTION;

-- Verification
SELECT 'Reactions' AS [Table], COUNT(*) AS [Count] FROM Reactions
UNION ALL SELECT 'Quizzes', COUNT(*) FROM Quizzes
UNION ALL SELECT 'Questions', COUNT(*) FROM Questions
UNION ALL SELECT 'QuizQuestions', COUNT(*) FROM QuizQuestions
UNION ALL SELECT 'STLInput', COUNT(*) FROM STLInput
UNION ALL SELECT 'STLAvailableReactions', COUNT(*) FROM STLAvailableReactions
UNION ALL SELECT 'STAvailableChemicals', COUNT(*) FROM STAvailableChemicals
UNION ALL SELECT 'STAvailableMethods', COUNT(*) FROM STAvailableMethods;
