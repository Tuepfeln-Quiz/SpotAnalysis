using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IStudentService
{
    public Task Register(string userName, string password, string? email, Guid? userId);
    public Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId);
    public Task<QuizDto> OpenQuiz(Guid studentId, int quizId);
    public Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult);
}