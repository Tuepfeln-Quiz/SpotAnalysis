using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IChemistryDataService
{
    Task<List<LabChemicalDto>> GetAllChemicalsAsync();
    Task<List<LabReactionDto>> GetAllReactionsAsync();
    Task<List<LightQuizDto>> GetLightQuizzesAsync();
    Task<List<SpotTestQuizDto>> GetSpotTestQuizzesAsync();
    Task<SpotTestQuizDto?> GetSpotTestQuizAsync(int quizId);
    Task<List<MethodQuestionDto>> GetAllMethodsAsync();
}
