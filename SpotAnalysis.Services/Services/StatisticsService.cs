using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class StatisticsService(IDbContextFactory<AnalysisContext> factory) : IStatisticsService
{
    public async Task<int> CreateAttemptAsync(Guid userId, int quizId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = new QuizAttempt
        {
            UserId = userId, QuizId = quizId, Started = DateTime.UtcNow, Completed = DateTime.MinValue
        };

        context.QuizAttempts.Add(attempt);
        await context.SaveChangesAsync();

        return attempt.AttemptId;
    }

    public async Task SaveLightResultAsync(int attemptId, int questionId, int chosenReactionId, bool isCorrect)
    {
        await using var context = await factory.CreateDbContextAsync();


        var result = new StlResult
        {
            AttemptId = attemptId,
            QuestionId = questionId,
            ChosenReactionId = chosenReactionId,
            IsCorrect = isCorrect
        };

        context.StlResults.Add(result);
        await context.SaveChangesAsync();
    }

    public async Task SaveTuepfelnResultAsync(int attemptID, int questionID,
        List<(int chemicalID, string formula, bool isCorrect)> answers)
    {
        await using var context = await factory.CreateDbContextAsync();

        var stResult = new StResult { AttemptId = attemptID, QuestionId = questionID };

        foreach (var (chemicalId, formula, isCorrect) in answers)
        {
            stResult.ChemicalResults.Add(new StChemicalResult
            {
                ChemicalId = chemicalId, ChosenFormula = formula, IsCorrect = isCorrect
            });
        }

        context.StResults.Add(stResult);
        await context.SaveChangesAsync();
    }

    public async Task CompleteAttemptAsync(int attemptId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = await context.QuizAttempts.FindAsync(attemptId);
        if (attempt != null)
        {
            attempt.Completed = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var stats = await context.QuizAttempts
            .Where(a => a.UserId == userId && a.Completed != null)
            .Select(a => new
            {
                HasLight = a.StlResults.Any(),
                HasST = a.StResults.Any(),
                LightCorrect = a.StlResults.Count(r => r.IsCorrect),
                LightTotal = a.StlResults.Count(),
                STCorrect = a.StResults.SelectMany(r => r.ChemicalResults).Count(c => c.IsCorrect),
                STTotal = a.StResults.SelectMany(r => r.ChemicalResults).Count()
            })
            .ToListAsync();

        var lightAttempts = stats.Count(s => s.HasLight);
        var stAttempts = stats.Count(s => s.HasST);
        var totalCorrect = stats.Sum(s => s.LightCorrect + s.STCorrect);
        var totalQuestions = stats.Sum(s => s.LightTotal + s.STTotal);

        return new UserStatisticsDto
        {
            TotalAttempts = stats.Count,
            LightAttempts = lightAttempts,
            TuepfelnAttempts = stAttempts,
            TotalCorrect = totalCorrect,
            TotalQuestions = totalQuestions
        };
    }

    public async Task<List<QuizHistoryDto>> GetUserHistoryAsync(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempts = await context.QuizAttempts
            .Where(a => a.UserId == userId && a.Completed != null)
            .Include(a => a.StResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.StlResults)
            .Include(a => a.Quiz).ThenInclude(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
            .OrderByDescending(a => a.Started)
            .ToListAsync();

        return attempts.Select(a => new QuizHistoryDto
        {
            AttemptId = a.AttemptId,
            QuizId = a.QuizId,
            QuizName = a.Quiz.Name,
            QuizType = DetermineQuizType(a),
            Started = a.Started,
            Completed = a.Completed,
            CorrectAnswers = CalculateCorrect(a),
            TotalQuestions = CalculateTotal(a)
        }).ToList();
    }

    private static QuestionType DetermineQuizType(QuizAttempt attempt)
    {
        if (attempt.Quiz?.QuizQuestions?.Any() == true)
        {
            var firstQuestion = attempt.Quiz.QuizQuestions.First().Question;
            return firstQuestion.Type;
        }

        if (attempt.StlResults.Any())
            return QuestionType.SpotTestLight;
        if (attempt.StResults.Any())
            return QuestionType.SpotTest;

        return QuestionType.SpotTest;
    }

    private static int CalculateCorrect(QuizAttempt attempt)
    {
        var correct = 0;

        foreach (var light in attempt.StlResults)
        {
            if (light.IsCorrect)
                correct++;
        }

        foreach (var st in attempt.StResults)
        {
            correct += st.ChemicalResults.Count(c => c.IsCorrect);
        }

        return correct;
    }

    private static int CalculateTotal(QuizAttempt attempt)
    {
        var total = 0;

        total += attempt.StlResults.Count;
        total += attempt.StResults.Sum(r => r.ChemicalResults.Count);

        return total;
    }
}
