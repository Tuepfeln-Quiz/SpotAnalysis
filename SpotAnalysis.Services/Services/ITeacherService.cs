using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface ITeacherService
{
    public Task<List<StudentDto>> GetStudents(int teacherId);
    public Task<List<StudentDto>> GetStudentsByGroup(int teacherId, int groupId);
    
    public Task<List<GroupDto>> GetGroups(int teacherId);
    public Task CreateGroup(int teacherId, ConfigGroupDto group);
    public Task UpdateGroup(int teacherId, ConfigGroupDto group);
    public Task DeleteGroup(int teacherId, int groupId);
    
    public Task AssignUserToGroup(int teacherId, int userId, int groupId);
    public Task RemoveUserFromGroup(int teacherId, int userId, int groupId);
    
    // public List<QuizDto> GetQuizzesByType(QuizTypeEnum type);
    // public void CreateQuiz(CreateQuizDto quiz);
    // public void UpdateQuiz(UpdateQuizDto quiz);
    // public void DeleteQuiz(int quizId);
    
    // public List<QuestionDto> GetQuestionsByType(QuestionTypeEnum type);
    // public void CreateQuestion(CreateQuestionDto question);
    // public void UpdateQuestion(UpdateQuestionDto question);
    // public void DeleteQuestion(int questionId);
}