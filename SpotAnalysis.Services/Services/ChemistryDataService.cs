using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class ChemistryDataService(IDbContextFactory<AnalysisContext> factory) : IChemistryDataService
{
    public async Task<List<LabChemicalDto>> GetAllChemicalsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        var chemicals = await context.Chemicals
            .Include(c => c.Color)
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Method)
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Color)
            .AsNoTracking()
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return chemicals.Select(MapChemical).ToList();
    }

    public async Task<List<LabReactionDto>> GetAllReactionsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.Reactions
            .Include(r => r.Chemical1)
            .Include(r => r.Chemical2)
            .Include(r => r.Observation)
            .AsNoTracking()
            .Select(r => new LabReactionDto
            {
                ReactionID = r.ReactionID,
                Chemical1ID = r.Chemical1ID,
                Chemical2ID = r.Chemical2ID,
                Chemical1Name = r.Chemical1.Name,
                Chemical2Name = r.Chemical2.Name,
                RelevantProduct = r.RelevantProduct,
                Formula = r.Formula,
                ObservationDescription = r.Observation.Description,
                ImagePath = r.ImagePath
            })
            .ToListAsync();
    }

    public async Task<List<MethodQuestionDto>> GetAllMethodsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Methods
            .AsNoTracking()
            .Select(m => new MethodQuestionDto { Id = m.MethodID, Name = m.Name }).ToListAsync();
    }

    public async Task<List<ColorDto>> GetAllColorsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Colors
            .AsNoTracking()
            .OrderBy(c => c.Name.Length)
            .ThenBy(c => c.Name)
            .Select(c => new ColorDto { Id = c.ColorId, Name = c.Name, HexValue = c.HexValue })
            .ToListAsync();
    }

    private static ColorDto MapColor(Color c) => new() { Id = c.ColorId, Name = c.Name, HexValue = c.HexValue };

    private static LabChemicalDto MapChemical(Chemical c) => new()
    {
        ChemicalID = c.ChemicalID,
        Name = c.Name,
        Formula = c.Formula,
        ImagePath = c.ImagePath,
        Type = c.Type,
        ChemicalTypeID = (int)c.Type,
        ChemicalTypeName = c.Type == ChemicalType.Educt ? "Edukt" : "Zusatzstoff",
        Color = MapColor(c.Color),
        MethodOutputs = c.MethodOutputs.ToDictionary(mo => mo.Method.Name, mo => MapColor(mo.Color))
    };
}
