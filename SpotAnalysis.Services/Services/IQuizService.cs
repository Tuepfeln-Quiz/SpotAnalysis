using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IQuizService
{
    public List<QuizDto> GetQuizzes();
    public void CreateQuiz(ConfigQuizDto quiz);
    public void UpdateQuiz(ConfigQuizDto quiz);
    public void DeleteQuiz(int quizId);
}