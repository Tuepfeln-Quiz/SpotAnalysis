using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Helpers;

namespace SpotAnalysis.Services.Services;

public class ChemistryDataService(IDbContextFactory<AnalysisContext> factory, HybridCache cache) : IChemistryDataService
{
    public async Task<List<LabChemicalDto>> GetAllChemicalsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        return await cache.GetOrCreateAsync(
            CacheHelper.AllChemicalsKey,
            async ct =>
            {
                var chemicals = await context.Chemicals
                    .Include(c => c.MethodOutputs)
                    .ThenInclude(mo => mo.Method)
                    .AsNoTracking()
                    .OrderBy(c => c.Type)
                    .ThenBy(c => c.Name)
                    .ToListAsync(ct);
        
                return chemicals.Select(MapChemical).ToList();
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(1)
            });
    }

    public async Task<List<LabReactionDto>> GetAllReactionsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        
        return await cache.GetOrCreateAsync(
            CacheHelper.AllReactionsKey,
            async ct =>
            {
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
                    .ToListAsync(ct);
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(1)
            });
    }

    public async Task<List<LightQuizDto>> GetLightQuizzesAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        var quizzes = await context.Quizzes
            .Where(q => q.QuizQuestions.Any(qq => qq.Question.Type == QuestionType.SpotTestLight))
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLQuestion!)
                        .ThenInclude(stl => stl.ShownEduct)
                            .ThenInclude(c => c.MethodOutputs)
                                .ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLQuestion!)
                        .ThenInclude(stl => stl.Reaction)
                            .ThenInclude(r => r.Observation)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLQuestion!)
                        .ThenInclude(stl => stl.AvailableReactions)
                            .ThenInclude(ar => ar.Reaction)
                                .ThenInclude(r => r.Chemical1)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLQuestion!)
                        .ThenInclude(stl => stl.AvailableReactions)
                            .ThenInclude(ar => ar.Reaction)
                                .ThenInclude(r => r.Chemical2)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLQuestion!)
                        .ThenInclude(stl => stl.AvailableReactions)
                            .ThenInclude(ar => ar.Reaction)
                                .ThenInclude(r => r.Observation)
            .AsNoTracking()
            .ToListAsync();

        return quizzes.Select(q => new LightQuizDto
        {
            QuizID = q.QuizID,
            Name = q.Name,
            Questions = q.QuizQuestions
                .Where(qq => qq.Question.Type == QuestionType.SpotTestLight)
                .OrderBy(qq => qq.Order)
                .Select(qq =>
                {
                    var stl = qq.Question.STLQuestion!;
                    var availableReactions = stl.AvailableReactions
                        .Select(ar => MapReaction(ar.Reaction))
                        .ToList();

                    return new LightQuestionDto
                    {
                        QuestionID = qq.QuestionID,
                        Description = qq.Question.Description,
                        Chemical = MapChemical(stl.ShownEduct),
                        ObservationDescription = stl.Reaction.Observation.Description,
                        AvailableReactions = availableReactions,
                        CorrectReactionID = stl.ReactionID
                    };
                })
                .ToList()
        }).ToList();
    }

    public async Task<List<SpotTestQuizDto>> GetSpotTestQuizzesAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        var quizzes = await context.Quizzes
            .Where(q => q.QuizQuestions.Any(qq => qq.Question.Type == QuestionType.SpotTest))
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STQuestion!)
                        .ThenInclude(st => st.AvailableChemicals)
                            .ThenInclude(ac => ac.Chemical)
                                .ThenInclude(c => c.MethodOutputs)
                                    .ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STQuestion!)
                        .ThenInclude(st => st.AvailableMethods)
                            .ThenInclude(am => am.Method)
            .AsNoTracking()
            .ToListAsync();

        return quizzes.Select(MapSpotTestQuiz).ToList();
    }

    public async Task<SpotTestQuizDto?> GetSpotTestQuizAsync(int quizId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var quiz = await context.Quizzes
            .Where(q => q.QuizID == quizId &&
                        q.QuizQuestions.Any(qq => qq.Question.Type == QuestionType.SpotTest))
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STQuestion!)
                        .ThenInclude(st => st.AvailableChemicals)
                            .ThenInclude(ac => ac.Chemical)
                                .ThenInclude(c => c.MethodOutputs)
                                    .ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STQuestion!)
                        .ThenInclude(st => st.AvailableMethods)
                            .ThenInclude(am => am.Method)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return quiz is null ? null : MapSpotTestQuiz(quiz);
    }

    public async Task<List<MethodQuestionDto>> GetAllMethodsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Methods
            .AsNoTracking()
            .Select(m => new MethodQuestionDto
            {
                Id = m.MethodID,
                Name = m.Name
            }).ToListAsync();
    }

    private static SpotTestQuizDto MapSpotTestQuiz(Data.Models.Quizzes.Quiz quiz) => new()
    {
        QuizID = quiz.QuizID,
        Name = quiz.Name,
        Questions = quiz.QuizQuestions
            .Where(qq => qq.Question.Type == QuestionType.SpotTest)
            .OrderBy(qq => qq.Order)
            .Select(qq =>
            {
                var st = qq.Question.STQuestion!;
                var allChems = st.AvailableChemicals;
                return new SpotTestQuestionDto
                {
                    QuestionID = qq.QuestionID,
                    Description = qq.Question.Description,
                    UnknownEducts = allChems
                        .Where(ac => ac.Chemical.Type == ChemicalType.Educt)
                        .Select(ac => MapChemical(ac.Chemical))
                        .ToList(),
                    AvailableChemicals = allChems
                        .Where(ac => ac.Chemical.Type == ChemicalType.Additive)
                        .Select(ac => MapChemical(ac.Chemical))
                        .ToList(),
                    AvailableMethods = st.AvailableMethods
                        .Select(am => am.Method.Name)
                        .ToList()
                };
            })
            .ToList()
    };

    private static LabChemicalDto MapChemical(Chemical c) => new()
    {
        ChemicalID = c.ChemicalID,
        Name = c.Name,
        Formula = c.Formula,
        ImagePath = c.ImagePath,
        Type = c.Type,
        ChemicalTypeID = (int)c.Type,
        ChemicalTypeName = c.Type == ChemicalType.Educt ? "Edukt" : "Zusatzstoff",
        Color = c.Color,
        MethodOutputs = c.MethodOutputs.ToDictionary(mo => mo.Method.Name, mo => mo.Color)
    };

    private static LabReactionDto MapReaction(Reaction r) => new()
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
    };
}
