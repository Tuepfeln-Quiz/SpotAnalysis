using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IQuizService
{
    Task<List<QuizOverviewDto>> GetAllQuizzes();
    Task<int> CreateQuiz(Guid teacherId, CreateQuizDto quiz);
    Task UpdateQuiz(Guid teacherId, UpdateQuizDto quiz);
    Task DeleteQuiz(Guid teacherId, int quizId);
    Task AssignGroupToQuiz(Guid teacherId, int quizId, int groupId);
    Task RemoveGroupFromQuiz(Guid teacherId, int quizId, int groupId);
    Task<List<GroupDto>> GetGroupsByQuiz(Guid teacherId, int quizId);

    Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId);
    Task<QuizPlayDto> StartOrResumeQuiz(Guid userId, int quizId);
    Task<QuizPlayDto> StartNewAttempt(Guid userId, int quizId);
    Task CompleteAttempt(Guid userId, int attemptId);
    Task<StlResult> ValidateAndSaveStlQuestion(ValidateStlQuestionDto answer);
    Task<StResult> ValidateAndSaveStQuestion(ValidateStQuestionDto answer);

    Task<List<QuestionOverviewDto>> GetQuestions();
    Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId);
    Task<QuestionDetailDto> GetQuestionDetail(int questionId);
    Task CreateSTQuestion(Guid teacherId, ConfigStQuestionDto question);
    Task CreateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question);
    Task UpdateSTQuestion(Guid teacherId, ConfigStQuestionDto question);
    Task UpdateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question);
    Task DeleteQuestion(Guid teacherId, int questionId);
}
