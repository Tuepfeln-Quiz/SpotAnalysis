namespace SpotAnalysis.Services.DTOs;

public class StudentDto
{
    public required int Id { get; init; }
    public required string UserName { get; init; }
    /// <summary>
    /// There will be the groups displayed where the corresponding teacher is also part of.
    /// Teachers will not see the groups where they are not part of.
    /// </summary>
    public List<GroupDto> AssignedGroups { get; init; }
}