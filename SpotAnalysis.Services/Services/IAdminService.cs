using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IAdminService
{
    public List<TeacherDto> GetTeachers();
    public void CreateTeacher(ConfigTeacherDto teacher);
    public void UpdateTeacher(ConfigTeacherDto teacher);
    public void DeleteTeacher(int teacherId);
}