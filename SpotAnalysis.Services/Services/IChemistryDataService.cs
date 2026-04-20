using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IChemistryDataService
{
    Task<List<LabChemicalDto>> GetAllChemicalsAsync();
    Task<List<LabReactionDto>> GetAllReactionsAsync();
    Task<List<MethodQuestionDto>> GetAllMethodsAsync();
}
