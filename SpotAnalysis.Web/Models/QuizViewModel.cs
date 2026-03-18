namespace SpotAnalysis.Web.Models;

public class QuizViewModel
{
    public int QuizID { get; set; }
    public string Name { get; set; } = "";
    public string QuizTypeName { get; set; } = "";
    public List<QuestionViewModel> Questions { get; set; } = new();
}

public class QuestionViewModel
{
    public int QuestionID { get; set; }
    public string Description { get; set; } = "";
    public List<ChemicalViewModel> AvailableChemicals { get; set; } = new();
    public List<string> AvailableMethods { get; set; } = new();
    public int CorrectChemical1ID { get; set; }
    public int CorrectChemical2ID { get; set; }
}
