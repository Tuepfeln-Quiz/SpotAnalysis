using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IStatisticsService
{
    Task<int> CreateAttemptAsync(Guid userID, int quizID);
    Task SaveLightResultAsync(int attemptID, int questionID, int chosenReactionID, bool isCorrect);
    Task SaveTuepfelnResultAsync(int attemptID, int questionID, List<(int chemicalID, string formula, bool isCorrect)> answers);
    Task CompleteAttemptAsync(int attemptID);
    Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userID);
    Task<List<QuizHistoryDto>> GetUserHistoryAsync(Guid userID);
    Task<List<StudentStatisticsDto>> GetGroupStudentStatisticsAsync(Guid requesterID, int groupID);
    Task<List<QuizHistoryDto>> GetGroupStudentHistoryAsync(Guid requesterID, int groupID, Guid studentID);
}