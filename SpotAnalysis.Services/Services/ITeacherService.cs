using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface ITeacherService
{
    public List<StudentDto> GetStudents(int teacherId);
    public List<StudentDto> GetStudentsByGroup(int teacherId, int groupId);
    
    public List<GroupDto> GetGroups(int teacherId);
    public void CreateGroup(int teacherId, ConfigGroupDto group);
    public void UpdateGroup(int teacherId, ConfigGroupDto group);
    public void DeleteGroup(int teacherId, int groupId);
    
    public void AssignUserToGroup(int userId, int groupId);
    public void RemoveUserFromGroup(int userId, int groupId);
    
    // public List<QuizDto> GetQuizzesByType(QuizTypeEnum type);
    // public void CreateQuiz(CreateQuizDto quiz);
    // public void UpdateQuiz(UpdateQuizDto quiz);
    // public void DeleteQuiz(int quizId);
    
    // public List<QuestionDto> GetQuestionsByType(QuestionTypeEnum type);
    // public void CreateQuestion(CreateQuestionDto question);
    // public void UpdateQuestion(UpdateQuestionDto question);
    // public void DeleteQuestion(int questionId);
}