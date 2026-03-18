namespace SpotAnalysis.Web.Models;

public static class SeedData
{
    // ── Chemicals ──────────────────────────────────────────────────────
    // 7 Edukte (TypeID=1) + 2 Zusatzstoffe (TypeID=2) aus Excel
    public static List<ChemicalViewModel> Chemicals { get; } = new()
    {
        // Edukte
        new() { ChemicalID = 1, Name = "Eisen(III)chlorid", Formula = "FeCl₃", Color = "#fbbf24", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "rot", ["Flammenfärbung"] = "keine" } },
        new() { ChemicalID = 2, Name = "Blei(II)nitrat", Formula = "Pb(NO₃)₂", Color = "#f5f5f5", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "orange", ["Flammenfärbung"] = "keine" } },
        new() { ChemicalID = 3, Name = "Kaliumiodid", Formula = "KI", Color = "#f5f5f5", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "grün", ["Flammenfärbung"] = "violett" } },
        new() { ChemicalID = 4, Name = "Natriumcarbonat", Formula = "NaCO₃", Color = "#f5f5f5", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "blau", ["Flammenfärbung"] = "orange" } },
        new() { ChemicalID = 5, Name = "Silber(I)nitrat", Formula = "AgNO₃", Color = "#f5f5f5", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "orange", ["Flammenfärbung"] = "keine" } },
        new() { ChemicalID = 6, Name = "Strontiumchlorid", Formula = "SrCl₂", Color = "#f5f5f5", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "orange", ["Flammenfärbung"] = "rot" } },
        new() { ChemicalID = 7, Name = "Bariumhydroxid", Formula = "Ba(OH)₂", Color = "#f5f5f5", ChemicalTypeID = 1, ChemicalTypeName = "Edukt",
            MethodOutputs = new() { ["pH-Papier"] = "blau", ["Flammenfärbung"] = "grün" } },

        // Zusatzstoffe
        new() { ChemicalID = 8, Name = "Natriumhydroxid", Formula = "NaOH", Color = "#dbeafe", ChemicalTypeID = 2, ChemicalTypeName = "Zusatzstoff" },
        new() { ChemicalID = 9, Name = "Salzsäure", Formula = "HCl", Color = "#e0f2fe", ChemicalTypeID = 2, ChemicalTypeName = "Zusatzstoff" },
    };

    // ── Reactions ──────────────────────────────────────────────────────
    // 24 Kombinationen mit Produkt aus Excel ("keines"-Einträge weggelassen)
    public static List<ReactionViewModel> Reactions { get; } = new()
    {
        // Edukt + Edukt
        new() { ReactionID = 1, Chemical1ID = 1, Chemical2ID = 2, Chemical1Name = "Eisen(III)chlorid", Chemical2Name = "Blei(II)nitrat",
            RelevantProduct = "Blei(II)chlorid", Formula = "FeCl₃ + Pb(NO₃)₂ → PbCl₂", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 2, Chemical1ID = 1, Chemical2ID = 3, Chemical1Name = "Eisen(III)chlorid", Chemical2Name = "Kaliumiodid",
            RelevantProduct = "Iod", Formula = "FeCl₃ + KI → I₂", ObservationDescription = "Orangebraune Färbung" },
        new() { ReactionID = 3, Chemical1ID = 1, Chemical2ID = 4, Chemical1Name = "Eisen(III)chlorid", Chemical2Name = "Natriumcarbonat",
            RelevantProduct = "Eisen(III)carbonat", Formula = "FeCl₃ + NaCO₃ → Fe₂(CO₃)₃", ObservationDescription = "Brauner Niederschlag" },
        new() { ReactionID = 4, Chemical1ID = 1, Chemical2ID = 5, Chemical1Name = "Eisen(III)chlorid", Chemical2Name = "Silber(I)nitrat",
            RelevantProduct = "Silber(I)chlorid", Formula = "FeCl₃ + AgNO₃ → AgCl", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 5, Chemical1ID = 1, Chemical2ID = 7, Chemical1Name = "Eisen(III)chlorid", Chemical2Name = "Bariumhydroxid",
            RelevantProduct = "Eisen(III)hydroxid", Formula = "FeCl₃ + Ba(OH)₂ → Fe(OH)₃", ObservationDescription = "Brauner Niederschlag" },
        new() { ReactionID = 6, Chemical1ID = 2, Chemical2ID = 3, Chemical1Name = "Blei(II)nitrat", Chemical2Name = "Kaliumiodid",
            RelevantProduct = "Blei(II)iodid", Formula = "Pb(NO₃)₂ + KI → PbI₂", ObservationDescription = "Gelber Niederschlag" },
        new() { ReactionID = 7, Chemical1ID = 2, Chemical2ID = 4, Chemical1Name = "Blei(II)nitrat", Chemical2Name = "Natriumcarbonat",
            RelevantProduct = "Blei(II)carbonat", Formula = "Pb(NO₃)₂ + NaCO₃ → PbCO₃", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 8, Chemical1ID = 2, Chemical2ID = 6, Chemical1Name = "Blei(II)nitrat", Chemical2Name = "Strontiumchlorid",
            RelevantProduct = "Blei(II)chlorid", Formula = "Pb(NO₃)₂ + SrCl₂ → PbCl₂", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 9, Chemical1ID = 2, Chemical2ID = 7, Chemical1Name = "Blei(II)nitrat", Chemical2Name = "Bariumhydroxid",
            RelevantProduct = "Blei(II)hydroxid", Formula = "Pb(NO₃)₂ + Ba(OH)₂ → Pb(OH)₂", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 10, Chemical1ID = 3, Chemical2ID = 5, Chemical1Name = "Kaliumiodid", Chemical2Name = "Silber(I)nitrat",
            RelevantProduct = "Silber(I)iodid", Formula = "KI + AgNO₃ → AgI", ObservationDescription = "Gelber Niederschlag" },
        new() { ReactionID = 11, Chemical1ID = 4, Chemical2ID = 5, Chemical1Name = "Natriumcarbonat", Chemical2Name = "Silber(I)nitrat",
            RelevantProduct = "Silber(I)carbonat", Formula = "NaCO₃ + AgNO₃ → Ag₂CO₃", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 12, Chemical1ID = 4, Chemical2ID = 6, Chemical1Name = "Natriumcarbonat", Chemical2Name = "Strontiumchlorid",
            RelevantProduct = "Strontiumcarbonat", Formula = "NaCO₃ + SrCl₂ → SrCO₃", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 13, Chemical1ID = 4, Chemical2ID = 7, Chemical1Name = "Natriumcarbonat", Chemical2Name = "Bariumhydroxid",
            RelevantProduct = "Bariumcarbonat", Formula = "NaCO₃ + Ba(OH)₂ → BaCO₃", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 14, Chemical1ID = 5, Chemical2ID = 6, Chemical1Name = "Silber(I)nitrat", Chemical2Name = "Strontiumchlorid",
            RelevantProduct = "Silber(I)chlorid", Formula = "AgNO₃ + SrCl₂ → AgCl", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 15, Chemical1ID = 5, Chemical2ID = 7, Chemical1Name = "Silber(I)nitrat", Chemical2Name = "Bariumhydroxid",
            RelevantProduct = "Silber(I)hydroxid", Formula = "AgNO₃ + Ba(OH)₂ → AgOH", ObservationDescription = "Brauner Niederschlag" },
        new() { ReactionID = 16, Chemical1ID = 6, Chemical2ID = 7, Chemical1Name = "Strontiumchlorid", Chemical2Name = "Bariumhydroxid",
            RelevantProduct = "Strontiumhydroxid", Formula = "SrCl₂ + Ba(OH)₂ → Sr(OH)₂", ObservationDescription = "Weißer Niederschlag" },

        // Edukt + Zusatzstoff (NaOH)
        new() { ReactionID = 17, Chemical1ID = 1, Chemical2ID = 8, Chemical1Name = "Eisen(III)chlorid", Chemical2Name = "Natriumhydroxid",
            RelevantProduct = "Eisen(III)hydroxid", Formula = "FeCl₃ + NaOH → Fe(OH)₃", ObservationDescription = "Brauner Niederschlag" },
        new() { ReactionID = 18, Chemical1ID = 2, Chemical2ID = 8, Chemical1Name = "Blei(II)nitrat", Chemical2Name = "Natriumhydroxid",
            RelevantProduct = "Blei(II)hydroxid", Formula = "Pb(NO₃)₂ + NaOH → Pb(OH)₂", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 19, Chemical1ID = 5, Chemical2ID = 8, Chemical1Name = "Silber(I)nitrat", Chemical2Name = "Natriumhydroxid",
            RelevantProduct = "Silber(I)hydroxid", Formula = "AgNO₃ + NaOH → AgOH", ObservationDescription = "Brauner Niederschlag" },
        new() { ReactionID = 20, Chemical1ID = 6, Chemical2ID = 8, Chemical1Name = "Strontiumchlorid", Chemical2Name = "Natriumhydroxid",
            RelevantProduct = "Strontiumhydroxid", Formula = "SrCl₂ + NaOH → Sr(OH)₂", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 21, Chemical1ID = 7, Chemical2ID = 8, Chemical1Name = "Bariumhydroxid", Chemical2Name = "Natriumhydroxid",
            RelevantProduct = "Bariumhydroxid", Formula = "Ba(OH)₂ + NaOH → Ba(OH)₂", ObservationDescription = "Häutchenbildung" },

        // Edukt + Zusatzstoff (HCl)
        new() { ReactionID = 22, Chemical1ID = 2, Chemical2ID = 9, Chemical1Name = "Blei(II)nitrat", Chemical2Name = "Salzsäure",
            RelevantProduct = "Blei(II)chlorid", Formula = "Pb(NO₃)₂ + HCl → PbCl₂", ObservationDescription = "Weißer Niederschlag" },
        new() { ReactionID = 23, Chemical1ID = 4, Chemical2ID = 9, Chemical1Name = "Natriumcarbonat", Chemical2Name = "Salzsäure",
            RelevantProduct = "Kohlenstoffdioxid", Formula = "NaCO₃ + HCl → CO₂", ObservationDescription = "Bläschenbildung" },
        new() { ReactionID = 24, Chemical1ID = 5, Chemical2ID = 9, Chemical1Name = "Silber(I)nitrat", Chemical2Name = "Salzsäure",
            RelevantProduct = "Silber(I)chlorid", Formula = "AgNO₃ + HCl → AgCl", ObservationDescription = "Weißer Niederschlag" },
    };

    // ── Quizzes ────────────────────────────────────────────────────────
    public static List<QuizViewModel> Quizzes { get; } = new()
    {
        new()
        {
            QuizID = 1, Name = "Chlorid-Nachweis", QuizTypeName = "Nachweis",
            Questions = new()
            {
                new() { QuestionID = 1, Description = "Weise Chlorid-Ionen nach, indem du Eisen(III)chlorid mit Silbernitrat mischst.",
                    AvailableChemicals = GetChemicals(1, 3, 4, 5),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 1, CorrectChemical2ID = 5 },
                new() { QuestionID = 2, Description = "Weise Chlorid-Ionen nach: Mische Silbernitrat mit Strontiumchlorid.",
                    AvailableChemicals = GetChemicals(2, 5, 6, 7),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 5, CorrectChemical2ID = 6 },
            }
        },
        new()
        {
            QuizID = 2, Name = "Iodid-Nachweis", QuizTypeName = "Nachweis",
            Questions = new()
            {
                new() { QuestionID = 3, Description = "Weise Iodid-Ionen mit Silbernitrat nach (gelber Niederschlag).",
                    AvailableChemicals = GetChemicals(3, 4, 5, 6),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 3, CorrectChemical2ID = 5 },
                new() { QuestionID = 4, Description = "Weise Iodid mit Eisen(III)chlorid nach (orangebraune Färbung).",
                    AvailableChemicals = GetChemicals(1, 2, 3, 7),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 1, CorrectChemical2ID = 3 },
            }
        },
        new()
        {
            QuizID = 3, Name = "Carbonat-Nachweis", QuizTypeName = "Nachweis",
            Questions = new()
            {
                new() { QuestionID = 5, Description = "Weise Carbonat-Ionen mit Salzsäure nach (Bläschenbildung).",
                    AvailableChemicals = GetChemicals(3, 4, 8, 9),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 4, CorrectChemical2ID = 9 },
                new() { QuestionID = 6, Description = "Fälle Carbonat mit Bariumhydroxid als weißen Niederschlag.",
                    AvailableChemicals = GetChemicals(4, 5, 6, 7),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 4, CorrectChemical2ID = 7 },
            }
        },
        new()
        {
            QuizID = 4, Name = "Blei-Nachweis", QuizTypeName = "Nachweis",
            Questions = new()
            {
                new() { QuestionID = 7, Description = "Weise Blei-Ionen mit Kaliumiodid nach (gelber Niederschlag).",
                    AvailableChemicals = GetChemicals(1, 2, 3, 4),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 2, CorrectChemical2ID = 3 },
                new() { QuestionID = 8, Description = "Fälle Blei als Chlorid mit Salzsäure.",
                    AvailableChemicals = GetChemicals(2, 5, 7, 9),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 2, CorrectChemical2ID = 9 },
            }
        },
        new()
        {
            QuizID = 5, Name = "Eisen-Nachweis", QuizTypeName = "Nachweis",
            Questions = new()
            {
                new() { QuestionID = 9, Description = "Weise Eisen(III)-Ionen mit Natronlauge nach (brauner Niederschlag).",
                    AvailableChemicals = GetChemicals(1, 3, 6, 8),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 1, CorrectChemical2ID = 8 },
                new() { QuestionID = 10, Description = "Weise Eisen(III) mit Bariumhydroxid nach.",
                    AvailableChemicals = GetChemicals(1, 2, 5, 7),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 1, CorrectChemical2ID = 7 },
            }
        },
        new()
        {
            QuizID = 6, Name = "Gemischte Nachweise", QuizTypeName = "Gemischt",
            Questions = new()
            {
                new() { QuestionID = 11, Description = "Weise Silber-Ionen mit Salzsäure nach.",
                    AvailableChemicals = GetChemicals(1, 4, 5, 9),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 5, CorrectChemical2ID = 9 },
                new() { QuestionID = 12, Description = "Fälle Strontium als Carbonat.",
                    AvailableChemicals = GetChemicals(2, 3, 4, 6),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 4, CorrectChemical2ID = 6 },
                new() { QuestionID = 13, Description = "Fälle Barium als Carbonat.",
                    AvailableChemicals = GetChemicals(1, 4, 5, 7),
                    AvailableMethods = new() { "Tüpfelprobe" },
                    CorrectChemical1ID = 4, CorrectChemical2ID = 7 },
            }
        },
    };

    // ── Light Quizzes ──────────────────────────────────────────────────
    public static List<LightQuizViewModel> LightQuizzes { get; } = new()
    {
        new()
        {
            QuizID = 101, Name = "Niederschläge erkennen",
            Questions = new()
            {
                new()
                {
                    QuestionID = 101, Description = "Welche Reaktion verursacht einen weißen Niederschlag mit Silbernitrat?",
                    Chemical = GetChemical(5),
                    ObservationDescription = "Weißer Niederschlag",
                    AvailableReactions = GetReactions(4, 10, 11, 14),
                    CorrectReactionID = 4 // FeCl₃ + AgNO₃ → AgCl
                },
                new()
                {
                    QuestionID = 102, Description = "Was führt zu einem gelben Niederschlag mit Blei(II)nitrat?",
                    Chemical = GetChemical(2),
                    ObservationDescription = "Gelber Niederschlag",
                    AvailableReactions = GetReactions(6, 7, 8, 9),
                    CorrectReactionID = 6 // Pb(NO₃)₂ + KI → PbI₂
                },
                new()
                {
                    QuestionID = 103, Description = "Welche Reaktion erzeugt braunen Niederschlag mit Eisen(III)chlorid?",
                    Chemical = GetChemical(1),
                    ObservationDescription = "Brauner Niederschlag",
                    AvailableReactions = GetReactions(2, 3, 5, 17),
                    CorrectReactionID = 5 // FeCl₃ + Ba(OH)₂ → Fe(OH)₃
                },
            }
        },
        new()
        {
            QuizID = 102, Name = "Farben zuordnen",
            Questions = new()
            {
                new()
                {
                    QuestionID = 104, Description = "Welche Reaktion zeigt orangebraune Färbung?",
                    Chemical = GetChemical(1),
                    ObservationDescription = "Orangebraune Färbung",
                    AvailableReactions = GetReactions(1, 2, 3, 4),
                    CorrectReactionID = 2 // FeCl₃ + KI → I₂
                },
                new()
                {
                    QuestionID = 105, Description = "Welche Reaktion zeigt braunen Niederschlag mit Natronlauge?",
                    Chemical = GetChemical(5),
                    ObservationDescription = "Brauner Niederschlag",
                    AvailableReactions = GetReactions(10, 14, 15, 19),
                    CorrectReactionID = 19 // AgNO₃ + NaOH → AgOH
                },
            }
        },
        new()
        {
            QuizID = 103, Name = "Gasbildung & Spezial",
            Questions = new()
            {
                new()
                {
                    QuestionID = 106, Description = "Welche Reaktion zeigt Bläschenbildung?",
                    Chemical = GetChemical(4),
                    ObservationDescription = "Bläschenbildung",
                    AvailableReactions = GetReactions(11, 12, 13, 23),
                    CorrectReactionID = 23 // NaCO₃ + HCl → CO₂
                },
                new()
                {
                    QuestionID = 107, Description = "Was verursacht Häutchenbildung?",
                    Chemical = GetChemical(7),
                    ObservationDescription = "Häutchenbildung",
                    AvailableReactions = GetReactions(13, 15, 16, 21),
                    CorrectReactionID = 21 // Ba(OH)₂ + NaOH → Ba(OH)₂
                },
                new()
                {
                    QuestionID = 108, Description = "Welche Reaktion erzeugt gelben Niederschlag mit Kaliumiodid?",
                    Chemical = GetChemical(3),
                    ObservationDescription = "Gelber Niederschlag",
                    AvailableReactions = GetReactions(2, 6, 10, 13),
                    CorrectReactionID = 10 // KI + AgNO₃ → AgI
                },
            }
        },
    };

    // ── Tüpfeln Quizzes ──────────────────────────────────────────────
    // Identifikations-Modus: Edukte sind unbekannt (nur Eigenfarbe sichtbar)
    public static List<TupfelnQuizViewModel> TupfelnQuizzes { get; } = new()
    {
        new()
        {
            QuizID = 1, Name = "Grundlagen",
            Questions = new()
            {
                new()
                {
                    QuestionID = 1,
                    Description = "Bestimme die drei unbekannten Edukte. Du darfst mischen, pH-Papier und Flammenfärbung verwenden.",
                    UnknownEducts = GetChemicals(1, 3, 5), // FeCl₃, KI, AgNO₃
                    AvailableMethods = new() { "pH-Papier", "Flammenfärbung" },
                    AvailableChemicals = GetChemicals(8, 9) // NaOH, HCl
                },
                new()
                {
                    QuestionID = 2,
                    Description = "Identifiziere diese zwei Edukte anhand ihrer Reaktionen und Eigenschaften.",
                    UnknownEducts = GetChemicals(2, 4), // Pb(NO₃)₂, NaCO₃
                    AvailableMethods = new() { "pH-Papier", "Flammenfärbung" },
                    AvailableChemicals = GetChemicals(8, 9)
                },
            }
        },
        new()
        {
            QuizID = 2, Name = "Mittelstufe",
            Questions = new()
            {
                new()
                {
                    QuestionID = 3,
                    Description = "Drei unbekannte Edukte – nur Natronlauge als Zusatzstoff verfügbar.",
                    UnknownEducts = GetChemicals(6, 7, 1), // SrCl₂, Ba(OH)₂, FeCl₃
                    AvailableMethods = new() { "pH-Papier", "Flammenfärbung" },
                    AvailableChemicals = GetChemicals(8) // nur NaOH
                },
                new()
                {
                    QuestionID = 4,
                    Description = "Identifiziere die Edukte – nur Flammenfärbung und Salzsäure erlaubt.",
                    UnknownEducts = GetChemicals(3, 5, 4), // KI, AgNO₃, NaCO₃
                    AvailableMethods = new() { "Flammenfärbung" },
                    AvailableChemicals = GetChemicals(9) // nur HCl
                },
            }
        },
        new()
        {
            QuizID = 3, Name = "Fortgeschritten",
            Questions = new()
            {
                new()
                {
                    QuestionID = 5,
                    Description = "Vier unbekannte Edukte – nutze alle verfügbaren Hilfsmittel.",
                    UnknownEducts = GetChemicals(2, 3, 5, 6), // Pb(NO₃)₂, KI, AgNO₃, SrCl₂
                    AvailableMethods = new() { "pH-Papier", "Flammenfärbung" },
                    AvailableChemicals = GetChemicals(8, 9)
                },
            }
        },
        new()
        {
            QuizID = 4, Name = "Experte",
            Questions = new()
            {
                new()
                {
                    QuestionID = 6,
                    Description = "Alle sieben Edukte sind unbekannt. Identifiziere sie alle!",
                    UnknownEducts = GetChemicals(1, 2, 3, 4, 5, 6, 7),
                    AvailableMethods = new() { "pH-Papier", "Flammenfärbung" },
                    AvailableChemicals = GetChemicals(8, 9)
                },
            }
        },
    };

    private static ChemicalViewModel GetChemical(int id)
    {
        return Chemicals.First(c => c.ChemicalID == id);
    }

    private static List<ReactionViewModel> GetReactions(params int[] ids)
    {
        return Reactions.Where(r => ids.Contains(r.ReactionID)).ToList();
    }

    private static List<ChemicalViewModel> GetChemicals(params int[] ids)
    {
        return Chemicals.Where(c => ids.Contains(c.ChemicalID)).ToList();
    }
}
