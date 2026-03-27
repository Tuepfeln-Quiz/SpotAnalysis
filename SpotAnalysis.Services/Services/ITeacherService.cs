using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface ITeacherService
{
    public Task<List<StudentDto>> GetStudents(Guid teacherId);
    public Task<List<StudentDto>> GetStudentsByGroup(Guid teacherId, int groupId);
    
    public Task<List<GroupDto>> GetGroups(Guid teacherId);
    public Task CreateGroup(Guid teacherId, ConfigGroupDto group);
    public Task UpdateGroup(Guid teacherId, ConfigGroupDto group);
    public Task DeleteGroup(Guid teacherId, int groupId);
    
    public Task AssignUserToGroup(Guid teacherId, Guid userId, int groupId);
    public Task RemoveUserFromGroup(Guid teacherId, Guid userId, int groupId);
    
    // public List<QuizDto> GetQuizzesByType(QuizTypeEnum type);
    // public void CreateQuiz(CreateQuizDto quiz);
    // public void UpdateQuiz(UpdateQuizDto quiz);
    // public void DeleteQuiz(int quizId);
    
    // public List<QuestionDto> GetQuestionsByType(QuestionTypeEnum type);
    // public void CreateQuestion(CreateQuestionDto question);
    // public void UpdateQuestion(UpdateQuestionDto question);
    // public void DeleteQuestion(int questionId);
}