namespace SpotAnalysis.Services.DTOs;

public class SpotTestQuizDto
{
    public int QuizID { get; set; }
    public string Name { get; set; } = "";
    public List<SpotTestQuestionDto> Questions { get; set; } = new();
}

public class SpotTestQuestionDto
{
    public int QuestionID { get; set; }
    public string Description { get; set; } = "";
    /// <summary>Chemicals to identify — shown only by number + Eigenfarbe, not by name.</summary>
    public List<LabChemicalDto> UnknownEducts { get; set; } = new();
    /// <summary>Available analysis methods (e.g. "pH-Papier", "Flammenfaerbung").</summary>
    public List<string> AvailableMethods { get; set; } = new();
    /// <summary>Additional known chemicals for mixing (e.g. NaOH, HCl).</summary>
    public List<LabChemicalDto> AvailableChemicals { get; set; } = new();
}
