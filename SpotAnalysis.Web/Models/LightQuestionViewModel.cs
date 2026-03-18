namespace SpotAnalysis.Web.Models;

public class LightQuizViewModel
{
    public int QuizID { get; set; }
    public string Name { get; set; } = "";
    public List<LightQuestionViewModel> Questions { get; set; } = new();
}

public class LightQuestionViewModel
{
    public int QuestionID { get; set; }
    public string Description { get; set; } = "";
    public ChemicalViewModel Chemical { get; set; } = default!;
    public string ObservationDescription { get; set; } = "";
    public List<ReactionViewModel> AvailableReactions { get; set; } = new();
    public int CorrectReactionID { get; set; }
}
