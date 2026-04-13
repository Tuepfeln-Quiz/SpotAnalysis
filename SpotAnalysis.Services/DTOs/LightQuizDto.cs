namespace SpotAnalysis.Services.DTOs;

public class LightQuizDto
{
    public int QuizID { get; set; }
    public string Name { get; set; } = "";
    public List<LightQuestionDto> Questions { get; set; } = new();
}

public class LightQuestionDto
{
    public int QuestionID { get; set; }
    public string Description { get; set; } = "";
    public LabChemicalDto Chemical { get; set; } = default!;
    public string ObservationDescription { get; set; } = "";
    public List<LabReactionDto> AvailableReactions { get; set; } = new();
    public int CorrectReactionID { get; set; }
}
