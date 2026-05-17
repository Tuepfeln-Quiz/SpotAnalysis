using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class MasterDataService(IDbContextFactory<AnalysisContext> factory) : IMasterDataService
{
    private static readonly Expression<Func<Chemical, ChemicalRow>> ChemicalProjection =
        c => new ChemicalRow(
            c.ChemicalId, c.Name, c.Formula, c.ColorId, c.Color.Name,
            c.Type, c.ImagePath,
            c.MethodOutputs
                .OrderBy(mo => mo.Method)
                .Select(mo => new MethodOutputEntry
                {
                    Method = mo.Method, ColorId = mo.ColorId, ColorName = mo.Color.Name
                }).ToList(),
            c.Chemical1Reactions
                .Select(r => new ReactionRef(r.ReactionId, r.Chemical1.Name, r.Chemical2.Name))
                .ToList(),
            c.Chemical2Reactions
                .Select(r => new ReactionRef(r.ReactionId, r.Chemical1.Name, r.Chemical2.Name))
                .ToList(),
            c.StAvailableChemicals
                .Select(sac => new QuizRef(sac.QuestionId, sac.StQuestion.Question.Title))
                .ToList(),
            c.StlQuestions
                .Select(stl => new QuizRef(stl.QuestionId, stl.Question.Title))
                .ToList()
        );

    private static readonly Expression<Func<Reaction, ReactionRow>> ReactionProjection =
        r => new ReactionRow(
            r.ReactionId, r.Chemical1.ChemicalId, r.Chemical2.ChemicalId,
            r.Chemical1.Name, r.Chemical2.Name,
            r.RelevantProduct, r.Formula,
            r.ObservationId, r.Observation!.Description,
            r.ImagePath,
            r.StlQuestions
                .Select(q => new QuizRef(q.QuestionId, q.Question.Title))
                .ToList(),
            r.StlAvailableReactions
                .Select(ar => new QuizRef(ar.QuestionId, ar.StlQuestion.Question.Title))
                .ToList()
        );

    public async Task<List<ChemicalDetailDto>> GetChemicalsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var rows = await context.Chemicals
            .AsNoTracking()
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .Select(ChemicalProjection)
            .ToListAsync(ct);

        return rows.ConvertAll(MapChemicalRow);
    }

    public async Task<ChemicalDetailDto?> GetChemicalByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var row = await context.Chemicals
            .AsNoTracking()
            .Where(c => c.ChemicalId == id)
            .Select(ChemicalProjection)
            .FirstOrDefaultAsync(ct);

        return row is null ? null : MapChemicalRow(row);
    }

    public async Task<int> CreateChemicalAsync(ChemicalDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var exists = await context.Chemicals.AnyAsync(c =>
            EF.Functions.ILike(c.Name, dto.Name) &&
            EF.Functions.ILike(c.Formula, dto.Formula), ct);
        if (exists)
            throw new InvalidOperationException(
                $"Eine Chemikalie mit Name \"{dto.Name}\" und Formel \"{dto.Formula}\" existiert bereits.");

        var colorId = await ResolveColorIdAsync(context, dto.ColorId, dto.ColorName, ct);

        var chemical = new Chemical
        {
            Name = dto.Name,
            Formula = dto.Formula,
            ColorId = colorId,
            Type = dto.Type,
            ImagePath = dto.ImagePath
        };
        context.Chemicals.Add(chemical);
        await context.SaveChangesAsync(ct);

        await UpsertMethodOutputsAsync(context, chemical, dto.MethodOutputs, ct);
        await context.SaveChangesAsync(ct);

        return chemical.ChemicalId;
    }

    public async Task UpdateChemicalAsync(ChemicalDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var chemical = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Id, ct)
                       ?? throw new InvalidOperationException($"Chemikalie {dto.Id} nicht gefunden.");

        var duplicate = await context.Chemicals.AnyAsync(c =>
            c.ChemicalId != dto.Id &&
            EF.Functions.ILike(c.Name, dto.Name) &&
            EF.Functions.ILike(c.Formula, dto.Formula), ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Eine andere Chemikalie mit Name \"{dto.Name}\" und Formel \"{dto.Formula}\" existiert bereits.");

        chemical.Name = dto.Name;
        chemical.Formula = dto.Formula;
        chemical.ColorId = await ResolveColorIdAsync(context, dto.ColorId, dto.ColorName, ct);
        chemical.Type = dto.Type;
        chemical.ImagePath = dto.ImagePath;

        await UpsertMethodOutputsAsync(context, chemical, dto.MethodOutputs, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteChemicalAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var chemical = await context.Chemicals
                           .Include(c => c.MethodOutputs)
                           .FirstOrDefaultAsync(c => c.ChemicalId == id, ct)
                       ?? throw new InvalidOperationException($"Chemikalie {id} nicht gefunden.");

        var refCount = await context.Chemicals
            .Where(c => c.ChemicalId == id)
            .Select(c =>
                c.Chemical1Reactions.Count() +
                c.Chemical2Reactions.Count() +
                c.StAvailableChemicals.Count() +
                c.StlQuestions.Count())
            .SingleAsync(ct);

        if (refCount > 0)
            throw new InvalidOperationException(
                $"Chemikalie wird noch referenziert ({refCount} Einträge) und kann nicht gelöscht werden.");

        context.MethodOutputs.RemoveRange(chemical.MethodOutputs);
        context.Chemicals.Remove(chemical);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<ReactionDetailDto>> GetReactionsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var rows = await context.Reactions
            .AsNoTracking()
            .OrderBy(r => r.Chemical1.Name)
            .ThenBy(r => r.Chemical2.Name)
            .Select(ReactionProjection)
            .ToListAsync(ct);

        return rows.ConvertAll(MapReactionRow);
    }

    public async Task<ReactionDetailDto?> GetReactionByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var row = await context.Reactions
            .AsNoTracking()
            .Where(r => r.ReactionId == id)
            .Select(ReactionProjection)
            .FirstOrDefaultAsync(ct);

        return row is null ? null : MapReactionRow(row);
    }

    public async Task<int> CreateReactionAsync(ReactionDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        if (dto.Chemical1Id == dto.Chemical2Id)
            throw new InvalidOperationException("Chemikalie A und Chemikalie B müssen unterschiedlich sein.");

        var chem1 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical1Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical1Id} nicht gefunden.");
        var chem2 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical2Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical2Id} nicht gefunden.");

        var (lowId, highId) = chem1.ChemicalId <= chem2.ChemicalId
            ? (chem1.ChemicalId, chem2.ChemicalId)
            : (chem2.ChemicalId, chem1.ChemicalId);

        var duplicate = await context.Reactions.AnyAsync(r =>
            r.Chemical1Id == lowId && r.Chemical2Id == highId, ct);
        if (duplicate)
            throw new InvalidOperationException(
                "Für diese Chemikalien-Kombination existiert bereits eine Reaktion.");

        var observation = await ResolveObservationAsync(context, dto, ct);

        var reaction = new Reaction(chem1, chem2)
        {
            RelevantProduct = dto.RelevantProduct,
            Formula = dto.Formula,
            ObservationId = observation.ObservationId,
            ImagePath = dto.ImagePath
        };
        context.Reactions.Add(reaction);
        await context.SaveChangesAsync(ct);

        return reaction.ReactionId;
    }

    public async Task UpdateReactionAsync(ReactionDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var reaction = await context.Reactions.FirstOrDefaultAsync(r => r.ReactionId == dto.Id, ct)
                       ?? throw new InvalidOperationException($"Reaktion {dto.Id} nicht gefunden.");

        if (dto.Chemical1Id == dto.Chemical2Id)
            throw new InvalidOperationException("Chemikalie A und Chemikalie B müssen unterschiedlich sein.");

        var chem1 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical1Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical1Id} nicht gefunden.");
        var chem2 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical2Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical2Id} nicht gefunden.");

        var (lowId, highId) = chem1.ChemicalId <= chem2.ChemicalId
            ? (chem1.ChemicalId, chem2.ChemicalId)
            : (chem2.ChemicalId, chem1.ChemicalId);

        var duplicate = await context.Reactions.AnyAsync(r =>
            r.ReactionId != dto.Id &&
            r.Chemical1Id == lowId && r.Chemical2Id == highId, ct);
        if (duplicate)
            throw new InvalidOperationException(
                "Für diese Chemikalien-Kombination existiert bereits eine andere Reaktion.");

        var observation = await ResolveObservationAsync(context, dto, ct);

        reaction.SetChemicals(chem1, chem2);
        reaction.RelevantProduct = dto.RelevantProduct;
        reaction.Formula = dto.Formula;
        reaction.ObservationId = observation.ObservationId;
        reaction.ImagePath = dto.ImagePath;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteReactionAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var reaction = await context.Reactions
                           .FirstOrDefaultAsync(r => r.ReactionId == id, ct)
                       ?? throw new InvalidOperationException($"Reaktion {id} nicht gefunden.");

        var refCount = await context.Reactions
            .Where(r => r.ReactionId == id)
            .Select(r => r.StlQuestions.Count() + r.StlAvailableReactions.Count())
            .SingleAsync(ct);

        if (refCount > 0)
            throw new InvalidOperationException(
                $"Reaktion wird noch referenziert ({refCount} Einträge) und kann nicht gelöscht werden.");

        context.Reactions.Remove(reaction);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<ObservationDetailDto>> GetObservationsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var rows = await context.Observations
            .AsNoTracking()
            .OrderBy(o => o.Description)
            .Select(o => new
            {
                o.ObservationId,
                o.Description,
                Reactions = o.Reactions
                    .Select(r => new ReactionRef(r.ReactionId, r.Chemical1.Name, r.Chemical2.Name))
                    .ToList()
            })
            .ToListAsync(ct);

        return rows.ConvertAll(o => new ObservationDetailDto
        {
            Id = o.ObservationId,
            Description = o.Description,
            References = new ReferenceReport
            {
                Items = o.Reactions.Select(r => new ReferenceItem
                {
                    Kind = "Reaction",
                    Id = r.ReactionId,
                    Description = $"Reaktion #{r.ReactionId}: {r.Chem1} + {r.Chem2}"
                }).ToList()
            }
        });
    }

    public async Task<int> CreateObservationAsync(ObservationDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var desc = dto.Description.Trim();

        var duplicate = await context.Observations.AnyAsync(o =>
            EF.Functions.ILike(o.Description, desc), ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Eine Beobachtung mit Beschreibung \"{desc}\" existiert bereits.");

        var obs = new Observation { Description = desc };
        context.Observations.Add(obs);
        await context.SaveChangesAsync(ct);
        return obs.ObservationId;
    }

    public async Task UpdateObservationAsync(ObservationDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var desc = dto.Description.Trim();

        var obs = await context.Observations.FirstOrDefaultAsync(o => o.ObservationId == dto.Id, ct)
                  ?? throw new InvalidOperationException($"Beobachtung {dto.Id} nicht gefunden.");

        var duplicate = await context.Observations.AnyAsync(o =>
            o.ObservationId != dto.Id && EF.Functions.ILike(o.Description, desc), ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Eine andere Beobachtung mit Beschreibung \"{desc}\" existiert bereits.");

        obs.Description = desc;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteObservationAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var obs = await context.Observations
                      .FirstOrDefaultAsync(o => o.ObservationId == id, ct)
                  ?? throw new InvalidOperationException($"Beobachtung {id} nicht gefunden.");

        var reactionCount = await context.Observations
            .Where(o => o.ObservationId == id)
            .Select(o => o.Reactions.Count())
            .SingleAsync(ct);

        if (reactionCount > 0)
            throw new InvalidOperationException(
                $"Beobachtung wird von {reactionCount} Reaktionen verwendet und kann nicht gelöscht werden.");

        context.Observations.Remove(obs);
        await context.SaveChangesAsync(ct);
    }


    private static ChemicalDetailDto MapChemicalRow(ChemicalRow c) => new()
    {
        Id = c.ChemicalId,
        Name = c.Name,
        Formula = c.Formula,
        ColorId = c.ColorId,
        ColorName = c.ColorName,
        Type = c.Type,
        ImagePath = c.ImagePath,
        MethodOutputs = c.MethodOutputs,
        References = new ReferenceReport
        {
            Items = c.Chem1Reactions.Concat(c.Chem2Reactions)
                .Select(r => new ReferenceItem
                {
                    Kind = "Reaction",
                    Id = r.ReactionId,
                    Description = $"Reaktion #{r.ReactionId}: {r.Chem1} + {r.Chem2}"
                })
                .Concat(c.StQuizRefs.Select(q => new ReferenceItem
                {
                    Kind = "Quiz",
                    Id = q.QuestionId,
                    Description = q.Title ?? $"Frage #{q.QuestionId}",
                    RouteTemplate = "/teacher/questions/{0}/edit?type=SpotTest"
                }))
                .Concat(c.StlQuizRefs.Select(q => new ReferenceItem
                {
                    Kind = "Quiz",
                    Id = q.QuestionId,
                    Description = q.Title ?? $"Frage #{q.QuestionId}",
                    RouteTemplate = "/teacher/questions/{0}/edit?type=SpotTestLight"
                }))
                .ToList()
        }
    };

    private static ReactionDetailDto MapReactionRow(ReactionRow r) => new()
    {
        Id = r.ReactionId,
        Chemical1Id = r.Chemical1Id,
        Chemical2Id = r.Chemical2Id,
        Chemical1Name = r.Chemical1Name,
        Chemical2Name = r.Chemical2Name,
        RelevantProduct = r.RelevantProduct,
        Formula = r.Formula,
        ObservationId = r.ObservationId == 0 ? null : r.ObservationId,
        ObservationDescription = r.ObservationDescription ?? "",
        ImagePath = r.ImagePath,
        References = new ReferenceReport
        {
            Items = r.StlQuestionRefs.Select(q => new ReferenceItem
                {
                    Kind = "Quiz",
                    Id = q.QuestionId,
                    Description = q.Title ?? $"Frage #{q.QuestionId}",
                    RouteTemplate = "/teacher/questions/{0}/edit?type=SpotTestLight"
                })
                .Concat(r.StlAvailableReactionRefs.Select(q => new ReferenceItem
                {
                    Kind = "Quiz", Id = q.QuestionId, Description = q.Title ?? $"Frage #{q.QuestionId}"
                }))
                .ToList()
        }
    };

    private static async Task<Observation> ResolveObservationAsync(
        AnalysisContext context, ReactionDetailDto dto, CancellationToken ct)
    {
        if (dto.ObservationId is { } id)
        {
            return await context.Observations.FirstOrDefaultAsync(o => o.ObservationId == id, ct)
                   ?? throw new InvalidOperationException($"Beobachtung {id} nicht gefunden.");
        }

        var desc = (dto.NewObservationDescription ?? "").Trim();
        if (string.IsNullOrEmpty(desc))
            throw new InvalidOperationException("Beobachtung muss ausgewählt oder eingegeben werden.");

        var existing = await context.Observations
            .FirstOrDefaultAsync(o => EF.Functions.ILike(o.Description, desc), ct);
        if (existing is not null)
            return existing;

        var created = new Observation { Description = desc };
        context.Observations.Add(created);
        await context.SaveChangesAsync(ct);
        return created;
    }

    private static async Task<int> ResolveColorIdAsync(
        AnalysisContext context, int colorId, string colorName, CancellationToken ct)
    {
        if (colorId > 0)
            return colorId;

        var color = await context.Colors.FirstOrDefaultAsync(
            c => EF.Functions.ILike(c.Name, colorName), ct);
        if (color is not null)
            return color.ColorId;

        color = new Color { Name = colorName, HexValue = "#666666" };
        context.Colors.Add(color);
        await context.SaveChangesAsync(ct);
        return color.ColorId;
    }

    private static async Task UpsertMethodOutputsAsync(
        AnalysisContext context, Chemical chemical, List<MethodOutputEntry> entries, CancellationToken ct)
    {
        var existingOutputs = await context.MethodOutputs
            .Where(mo => mo.ChemicalId == chemical.ChemicalId)
            .ToListAsync(ct);

        foreach (var entry in entries)
        {
            var existing = existingOutputs.FirstOrDefault(mo => mo.Method == entry.Method);

            if (entry.ColorId == 0 && string.IsNullOrWhiteSpace(entry.ColorName))
            {
                if (existing is not null)
                    context.MethodOutputs.Remove(existing);
            }
            else
            {
                var colorId = await ResolveColorIdAsync(context, entry.ColorId, entry.ColorName, ct);
                if (existing is null)
                {
                    context.MethodOutputs.Add(new MethodOutput
                    {
                        ChemicalId = chemical.ChemicalId, Method = entry.Method, ColorId = colorId
                    });
                }
                else
                {
                    existing.ColorId = colorId;
                }
            }
        }
    }

    private sealed record ReactionRef(int ReactionId, string Chem1, string Chem2);

    private sealed record QuizRef(int QuestionId, string? Title);

    private sealed record ChemicalRow(
        int ChemicalId,
        string Name,
        string Formula,
        int ColorId,
        string ColorName,
        ChemicalType Type,
        string? ImagePath,
        List<MethodOutputEntry> MethodOutputs,
        List<ReactionRef> Chem1Reactions,
        List<ReactionRef> Chem2Reactions,
        List<QuizRef> StQuizRefs,
        List<QuizRef> StlQuizRefs);

    private sealed record ReactionRow(
        int ReactionId,
        int Chemical1Id,
        int Chemical2Id,
        string Chemical1Name,
        string Chemical2Name,
        string RelevantProduct,
        string Formula,
        int ObservationId,
        string? ObservationDescription,
        string? ImagePath,
        List<QuizRef> StlQuestionRefs,
        List<QuizRef> StlAvailableReactionRefs);
}
