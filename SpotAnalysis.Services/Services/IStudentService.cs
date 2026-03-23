namespace SpotAnalysis.Services;

public interface IStudentService
{
    public void Register(string password, string? code);
    // public List<QuizDto> GetQuizzesByType(QuizTypeEnum type);
    // public List<QuestionDto> JoinQuiz(int quizId);
    // public QuestionResultDto ValidateQuestion(QuestionInputDto userInput);
}