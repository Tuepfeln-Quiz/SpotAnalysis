using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IQuizService
{
    public List<QuizDto> GetAllQuizzes();
    public void CreateQuiz(ConfigQuizDto quiz);
    public void UpdateQuiz(ConfigQuizDto quiz);
    public void DeleteQuiz(int quizId);
    
    public Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId);
    public Task<QuizDto> OpenQuiz(Guid studentId, int quizId);
    public Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult);
    
    public List<QuestionOverviewDto> GetQuestions();
    public List<QuestionOverviewDto> GetQuestionsOfQuiz(int quizId);
    public void CreateSTQuestion(ConfigSTQuestionDto question);
    public void CreateSTLQuestion(ConfigSTLQuestionDto question);
    public void UpdateSTQuestion(ConfigSTQuestionDto question);
    public void UpdateSTLQuestion(ConfigSTLQuestionDto question);
    public void DeleteQuestion(int questionId);
}