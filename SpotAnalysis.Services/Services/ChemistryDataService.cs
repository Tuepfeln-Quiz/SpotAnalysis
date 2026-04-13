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
            .Include(c => c.MethodOutputs)
                .ThenInclude(mo => mo.Method)
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

    public async Task<List<LightQuizDto>> GetLightQuizzesAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        var quizzes = await context.Quizzes
            .Where(q => q.QuizQuestions.Any(qq => qq.Question.Type == QuestionType.SpotTestLight))
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLInput)
                        .ThenInclude(i => i!.Chemical1)
                            .ThenInclude(c => c.MethodOutputs)
                                .ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLInput)
                        .ThenInclude(i => i!.Observation)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLAvailableReactions)
                        .ThenInclude(ar => ar.Reaction)
                            .ThenInclude(r => r.Chemical1)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLAvailableReactions)
                        .ThenInclude(ar => ar.Reaction)
                            .ThenInclude(r => r.Chemical2)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STLAvailableReactions)
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
                    var input = qq.Question.STLInput!;
                    var availableReactions = qq.Question.STLAvailableReactions
                        .Select(ar => MapReaction(ar.Reaction))
                        .ToList();

                    // Derive correct reaction: matches both the input chemical and observation
                    var correctReaction = qq.Question.STLAvailableReactions
                        .FirstOrDefault(ar =>
                            ar.Reaction.ObservationID == input.ObservationID &&
                            (ar.Reaction.Chemical1ID == input.Chemical1ID ||
                             ar.Reaction.Chemical2ID == input.Chemical2ID));

                    return new LightQuestionDto
                    {
                        QuestionID = qq.QuestionID,
                        Description = qq.Question.Description,
                        Chemical = MapChemical(input.Chemical1),
                        ObservationDescription = input.Observation.Description,
                        AvailableReactions = availableReactions,
                        CorrectReactionID = correctReaction?.ReactionID ?? 0
                    };
                })
                .ToList()
        }).ToList();
    }

    public async Task<List<SpotTestQuizDto>> GetSpotTestQuizzesAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        var quizzes = await context.Quizzes
            .Where(q => q.QuizQuestions.Any(qq => qq.Question.Type == QuestionType.Tuepfeln))
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STAvailableChemicals)
                        .ThenInclude(ac => ac.Chemical)
                            .ThenInclude(c => c.MethodOutputs)
                                .ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STAvailableMethods)
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
                        q.QuizQuestions.Any(qq => qq.Question.Type == QuestionType.Tuepfeln))
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STAvailableChemicals)
                        .ThenInclude(ac => ac.Chemical)
                            .ThenInclude(c => c.MethodOutputs)
                                .ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(question => question.STAvailableMethods)
                        .ThenInclude(am => am.Method)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return quiz is null ? null : MapSpotTestQuiz(quiz);
    }

    private static SpotTestQuizDto MapSpotTestQuiz(Data.Models.Quizzes.Quiz quiz) => new()
    {
        QuizID = quiz.QuizID,
        Name = quiz.Name,
        Questions = quiz.QuizQuestions
            .Where(qq => qq.Question.Type == QuestionType.Tuepfeln)
            .OrderBy(qq => qq.Order)
            .Select(qq =>
            {
                var allChems = qq.Question.STAvailableChemicals;
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
                    AvailableMethods = qq.Question.STAvailableMethods
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
        ChemicalTypeID = (int)c.Type + 1,
        ChemicalTypeName = c.Type == ChemicalType.Educt ? "Edukt" : "Zusatzstoff",
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
