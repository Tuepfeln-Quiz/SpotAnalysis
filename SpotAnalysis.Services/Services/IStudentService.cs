using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IStudentService
{
    public User? Register(string userName, string password, string? email);
    public List<QuizOverviewDto> GetQuizzes(Guid studentId);
    public List<QuizDto> OpenQuiz(Guid studentId, int quizId);
    public void ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult);
}