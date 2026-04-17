using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IQuizService
{
    public Task<List<QuizOverviewDto>> GetAllQuizzes();
    public Task<int> CreateQuiz(Guid teacherId, CreateQuizDto quiz);
    public Task UpdateQuiz(Guid teacherId, UpdateQuizDto quiz);
    public Task DeleteQuiz(Guid teacherId, int quizId);
    public Task AssignGroupToQuiz(int quizId, int groupId);
    public Task RemoveGroupFromQuiz(int quizId, int groupId);
    public Task<List<GroupDto>> GetGroupsByQuiz(int quizId);
    
    public Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId);
    public Task<QuizDto> OpenQuiz(Guid studentId, int quizId);
    public Task<STLResult> ValidateAndSaveStlQuestion(ValidateStlQuestionDto answer);
    public Task<STResult> ValidateAndSaveStQuestion(ValidateStQuestionDto answer);
    public Task<QuizAttempt?> GetQuizAttempt(Guid studentId, int quizId);
    
    public Task<List<QuestionOverviewDto>> GetQuestions();
    public Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId);
    public Task<QuestionDetailDto> GetQuestionDetail(int questionId);
    public Task CreateSTQuestion(Guid teacherId, ConfigSTQuestionDto question);
    public Task CreateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question);
    public Task UpdateSTQuestion(Guid teacherId, ConfigSTQuestionDto question);
    public Task UpdateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question);
    public Task DeleteQuestion(int questionId);
}