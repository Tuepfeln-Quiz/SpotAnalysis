namespace SpotAnalysis.Services.Services;
using SpotAnalysis.Data.Models.Identity;

public interface IStudentService
{
    public User Register(string password, string? code);
    // public List<QuizDto> GetQuizzesByType(QuizTypeEnum type);
    // public List<QuestionDto> JoinQuiz(int quizId);
    // public QuestionResultDto ValidateQuestion(QuestionInputDto userInput);
}