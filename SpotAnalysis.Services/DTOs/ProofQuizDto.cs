namespace SpotAnalysis.Services.DTOs;

public class ProofQuizDto
{
    public int QuizID { get; set; }
    public string Name { get; set; } = "";
    public string QuizTypeName { get; set; } = "";
    public List<ProofQuestionDto> Questions { get; set; } = new();
}

public class ProofQuestionDto
{
    public int QuestionID { get; set; }
    public string Description { get; set; } = "";
    public List<LabChemicalDto> AvailableChemicals { get; set; } = new();
    public List<string> AvailableMethods { get; set; } = new();
    public int CorrectChemical1ID { get; set; }
    public int CorrectChemical2ID { get; set; }
}
