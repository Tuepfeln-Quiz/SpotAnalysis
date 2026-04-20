using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class StatisticsService(IDbContextFactory<AnalysisContext> factory) : IStatisticsService
{
    public async Task<int> CreateAttemptAsync(Guid userId, int quizId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = new QuizAttempt
        {
            UserID = userId,
            QuizID = quizId,
            Started = DateTime.Now,
            Completed = DateTime.MinValue
        };

        context.QuizAttempts.Add(attempt);
        await context.SaveChangesAsync();

        return attempt.AttemptID;
    }
    public async Task SaveLightResultAsync(int attemptId, int questionId, int chosenReactionId, bool isCorrect)
    {
        await using var context = await factory.CreateDbContextAsync();


        var result = new STLResult
        {
            AttemptID = attemptId,
            QuestionID = questionId,
            ChosenReactionID = chosenReactionId,
            IsCorrect = isCorrect
        };

        context.STLResults.Add(result);
        await context.SaveChangesAsync();
    }
    public async Task SaveTuepfelnResultAsync(int attemptID, int questionID,
        List<(int chemicalID, string formula, bool isCorrect)> answers)
    {
        await using var context = await factory.CreateDbContextAsync();

        var stResult = new STResult
        {
            AttemptID = attemptID,
            QuestionID = questionID
        };

        foreach (var (chemicalId, formula, isCorrect) in answers)
        {
            stResult.ChemicalResults.Add(new STChemicalResult
            {
                ChemicalID = chemicalId,
                ChosenFormula = formula,
                IsCorrect = isCorrect
            });
        }

        context.STResults.Add(stResult);
        await context.SaveChangesAsync();
    }
    public async Task CompleteAttemptAsync(int attemptId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = await context.QuizAttempts.FindAsync(attemptId);
        if (attempt != null)
        {
            attempt.Completed = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
    public async Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var stats = await context.QuizAttempts
            .Where(a => a.UserID == userId && a.Completed != null)
            .Select(a => new
            {
                HasLight = a.STLResults.Any(),
                HasST = a.STResults.Any(),
                LightCorrect = a.STLResults.Count(r => r.IsCorrect),
                LightTotal = a.STLResults.Count(),
                STCorrect = a.STResults.SelectMany(r => r.ChemicalResults).Count(c => c.IsCorrect),
                STTotal = a.STResults.SelectMany(r => r.ChemicalResults).Count()
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
            .Where(a => a.UserID == userId && a.Completed != null)
            .Include(a => a.STResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.STLResults)
            .Include(a => a.Quiz).ThenInclude(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
            .OrderByDescending(a => a.Started)
            .ToListAsync();

        return attempts.Select(a => new QuizHistoryDto
        {
            AttemptId = a.AttemptID,
            QuizId = a.QuizID,
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

        if (attempt.STLResults.Any())
            return QuestionType.SpotTestLight;
        if (attempt.STResults.Any())
            return QuestionType.SpotTest;

        return QuestionType.SpotTest;
    }
    private static int CalculateCorrect(QuizAttempt attempt)
    {
        var correct = 0;

        foreach (var light in attempt.STLResults)
        {
            if (light.IsCorrect) correct++;
        }

        foreach (var st in attempt.STResults)
        {
            correct += st.ChemicalResults.Count(c => c.IsCorrect);
        }

        return correct;
    }
    private static int CalculateTotal(QuizAttempt attempt)
    {
        var total = 0;

        total += attempt.STLResults.Count;
        total += attempt.STResults.Sum(r => r.ChemicalResults.Count);

        return total;
    }
}