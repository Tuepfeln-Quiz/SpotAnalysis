using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IGroupService
{
    // Queries
    Task<List<GroupDto>> GetGroups(Guid teacherId);
    Task<List<StudentDto>> GetStudents(Guid teacherId);
    Task<List<StudentDto>> GetStudentsByGroup(Guid teacherId, int groupId);
    Task<List<StudentDto>> GetTeachersByGroup(Guid teacherId, int groupId);
    Task<List<StudentDto>> SearchTeachers(string query);
    Task<List<StudentDto>> SearchStudents(string query);
    Task<List<QuizOverviewDto>> GetQuizzesByGroup(int groupId);

    // CRUD
    Task CreateGroup(Guid teacherId, ConfigGroupDto group);
    Task UpdateGroup(Guid teacherId, ConfigGroupDto group);
    Task DeleteGroup(Guid teacherId, int groupId);

    // Membership (teacher-authorized)
    Task AssignUserToGroup(Guid teacherId, Guid userId, int groupId);
    Task RemoveUserFromGroup(Guid teacherId, Guid userId, int groupId);

    // Membership (admin — no teacher-auth check)
    Task AssignUserToGroup(Guid userId, int groupId);
    Task RemoveUserFromGroup(Guid userId, int groupId);

    // Token-based join
    Task<JoinGroupResult> JoinGroupByToken(Guid userId, string token);
}
