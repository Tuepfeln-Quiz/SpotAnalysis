namespace SpotAnalysis.Web.Models;

public class TuepfelnQuizViewModel
{
    public int QuizID { get; set; }
    public string Name { get; set; } = "";
    public List<TuepfelnQuestionViewModel> Questions { get; set; } = new();
}

public class TuepfelnQuestionViewModel
{
    public int QuestionID { get; set; }
    public string Description { get; set; } = "";
    /// <summary>Chemicals to identify — shown only by number + Eigenfarbe, not by name.</summary>
    public List<ChemicalViewModel> UnknownEducts { get; set; } = new();
    /// <summary>Available analysis methods (e.g. "pH-Papier", "Flammenfärbung").</summary>
    public List<string> AvailableMethods { get; set; } = new();
    /// <summary>Additional known chemicals for mixing (e.g. NaOH, HCl).</summary>
    public List<ChemicalViewModel> AvailableChemicals { get; set; } = new();
}
