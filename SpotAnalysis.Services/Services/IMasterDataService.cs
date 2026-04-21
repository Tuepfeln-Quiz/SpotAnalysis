using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IMasterDataService
{
    // Chemicals
    Task<List<ChemicalDetailDto>> GetChemicalsAsync(CancellationToken ct = default);
    Task<ChemicalDetailDto?> GetChemicalByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateChemicalAsync(ChemicalDetailDto dto, CancellationToken ct = default);
    Task UpdateChemicalAsync(ChemicalDetailDto dto, CancellationToken ct = default);
    Task DeleteChemicalAsync(int id, CancellationToken ct = default);

    // Reactions
    Task<List<ReactionDetailDto>> GetReactionsAsync(CancellationToken ct = default);
    Task<ReactionDetailDto?> GetReactionByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateReactionAsync(ReactionDetailDto dto, CancellationToken ct = default);
    Task UpdateReactionAsync(ReactionDetailDto dto, CancellationToken ct = default);
    Task DeleteReactionAsync(int id, CancellationToken ct = default);

    // Observations
    Task<List<ObservationDetailDto>> GetObservationsAsync(CancellationToken ct = default);
    Task<int> CreateObservationAsync(ObservationDetailDto dto, CancellationToken ct = default);
    Task UpdateObservationAsync(ObservationDetailDto dto, CancellationToken ct = default);
    Task DeleteObservationAsync(int id, CancellationToken ct = default);
}
