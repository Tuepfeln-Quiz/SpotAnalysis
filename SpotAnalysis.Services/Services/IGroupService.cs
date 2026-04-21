using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IGroupService
{
    // Queries
    Task<List<GroupDto>> GetGroups(Guid userId);
    Task<List<StudentDto>> GetStudents(Guid userId);
    Task<List<StudentDto>> GetStudentsByGroup(Guid userId, int groupId);
    Task<List<StudentDto>> GetTeachersByGroup(Guid userId, int groupId);
    Task<List<StudentDto>> SearchTeachers(string query);
    Task<List<StudentDto>> SearchStudents(string query);
    Task<List<QuizOverviewDto>> GetQuizzesByGroup(int groupId);

    // CRUD
    Task CreateGroup(Guid userId, ConfigGroupDto group);
    Task UpdateGroup(Guid userId, ConfigGroupDto group);
    Task DeleteGroup(Guid userId, int groupId);

    // Membership (teacher/admin-authorized)
    Task AssignUserToGroup(Guid actorId, Guid userId, int groupId);
    Task RemoveUserFromGroup(Guid actorId, Guid userId, int groupId);

    // Membership (admin — no teacher-auth check)
    Task AssignUserToGroup(Guid userId, int groupId);
    Task RemoveUserFromGroup(Guid userId, int groupId);

    // Token-based join
    Task<JoinGroupResult> JoinGroupByToken(Guid userId, string token);
}
