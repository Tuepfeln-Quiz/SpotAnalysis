using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IQuizService
{
    public List<QuizDto> GetQuizzes();
    public void CreateQuiz(ConfigQuizDto quiz);
    public void UpdateQuiz(ConfigQuizDto quiz);
    public void DeleteQuiz(int quizId);
    public Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId);
    public Task<QuizDto> OpenQuiz(Guid studentId, int quizId);
    public Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult);
}