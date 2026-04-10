using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IQuizService
{
    public Task<List<QuizOverviewDto>> GetAllQuizzes();
    public Task CreateQuiz(Guid createdBy, ConfigQuizDto quiz);
    public Task UpdateQuiz(ConfigQuizDto quiz);
    public Task DeleteQuiz(int quizId);
    
    public Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId);
    public Task<QuizDto> OpenQuiz(Guid studentId, int quizId);
    public Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult);
    
    public Task<List<QuestionOverviewDto>> GetQuestions();
    public Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId);
    public Task CreateSTQuestion(ConfigSTQuestionDto question);
    public Task CreateSTLQuestion(ConfigSTLQuestionDto question);
    public Task UpdateSTQuestion(ConfigSTQuestionDto question);
    public Task UpdateSTLQuestion(ConfigSTLQuestionDto question);
    public Task DeleteQuestion(int questionId);
}