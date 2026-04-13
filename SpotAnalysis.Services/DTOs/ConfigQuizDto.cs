namespace SpotAnalysis.Services.DTOs;

public class ConfigQuizDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required List<QuestionDto> Questions { get; set; }
    public required List<int> AssignedGroupsIds { get; set; }
}