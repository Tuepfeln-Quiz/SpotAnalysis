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
        
        var attempts = await context.QuizAttempts
            .Where(a => a.UserID == userId && a.Completed != DateTime.MinValue)
            .Include(a => a.STResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.STLResults)
            .Include(a => a.Quiz).ThenInclude(q => q.QuizQuestions)
            .ToListAsync();
        
        var lightAttempts = attempts.Where(a => a.STLResults.Any()).ToList();
        var stAttempts = attempts.Where(a => a.STResults.Any()).ToList();
        
        var totalCorrect = 0;
        var totalQuestions = 0;
        
        foreach (var light in lightAttempts)
        {
            totalCorrect += light.STLResults.Count(r => r.IsCorrect);
            totalQuestions += light.STLResults.Count;
        }
        
        foreach (var st in stAttempts)
        {
            totalCorrect += st.STResults.Sum(r => r.ChemicalResults.Count(c => c.IsCorrect));
            totalQuestions += st.STResults.Sum(r => r.ChemicalResults.Count);
        }
        
        return new UserStatisticsDto
        {
            TotalAttempts = attempts.Count,
            LightAttempts = lightAttempts.Count,
            TuepfelnAttempts = stAttempts.Count,
            TotalCorrect = totalCorrect,
            TotalQuestions = totalQuestions
        };
    }
    public async Task<List<QuizHistoryDto>> GetUserHistoryAsync(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var attempts = await context.QuizAttempts
            .Where(a => a.UserID == userId && a.Completed != DateTime.MinValue)
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
            Completed = a.Completed == DateTime.MinValue ? null : a.Completed,
            CorrectAnswers = CalculateCorrect(a),
            TotalQuestions = CalculateTotal(a)
        }).ToList();
    }

    public async Task<List<StudentStatisticsDto>> GetGroupStudentStatisticsAsync(Guid requesterId, int groupId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var authorizedStudentIds = await GetAuthorizedStudentIdsAsync(context, requesterId, groupId);
        if (authorizedStudentIds.Count == 0)
        {
            return [];
        }

        var students = await context.Users
            .Where(u => authorizedStudentIds.Contains(u.UserID))
            .Select(u => new
            {
                u.UserID,
                u.UserName
            })
            .ToListAsync();

        var attempts = await context.QuizAttempts
            .Where(a => authorizedStudentIds.Contains(a.UserID) && a.Completed != DateTime.MinValue)
            .Include(a => a.STResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.STLResults)
            .ToListAsync();

        var attemptsByStudent = attempts
            .GroupBy(a => a.UserID)
            .ToDictionary(g => g.Key, g => g.ToList());

        return students
            .Select(student =>
            {
                var studentAttempts = attemptsByStudent.GetValueOrDefault(student.UserID, []);
                var totalCorrect = studentAttempts.Sum(CalculateCorrect);
                var totalQuestions = studentAttempts.Sum(CalculateTotal);
                var lastAttemptAt = studentAttempts
                    .Where(a => a.Completed != DateTime.MinValue)
                    .OrderByDescending(a => a.Completed)
                    .Select(a => (DateTime?)a.Completed)
                    .FirstOrDefault();

                return new StudentStatisticsDto
                {
                    StudentId = student.UserID,
                    UserName = student.UserName,
                    TotalAttempts = studentAttempts.Count,
                    TotalCorrect = totalCorrect,
                    TotalQuestions = totalQuestions,
                    LastAttemptAt = lastAttemptAt
                };
            })
            .OrderByDescending(x => x.LastAttemptAt)
            .ThenBy(x => x.UserName)
            .ToList();
    }

    public async Task<List<QuizHistoryDto>> GetGroupStudentHistoryAsync(Guid requesterId, int groupId, Guid studentId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var authorizedStudentIds = await GetAuthorizedStudentIdsAsync(context, requesterId, groupId);
        if (!authorizedStudentIds.Contains(studentId))
        {
            return [];
        }

        var attempts = await context.QuizAttempts
            .Where(a => a.UserID == studentId && a.Completed != DateTime.MinValue)
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
            Completed = a.Completed == DateTime.MinValue ? null : a.Completed,
            CorrectAnswers = CalculateCorrect(a),
            TotalQuestions = CalculateTotal(a)
        }).ToList();
    }

    private static async Task<List<Guid>> GetAuthorizedStudentIdsAsync(AnalysisContext context, Guid requesterId, int groupId)
    {
        var requester = await context.Users
            .Where(u => u.UserID == requesterId)
            .Select(u => new
            {
                IsAdmin = u.Roles.Any(r => r == Role.Admin),
                IsTeacherInGroup = u.Roles.Any(r => r == Role.Teacher) && u.Groups.Any(g => g.GroupID == groupId)
            })
            .SingleOrDefaultAsync();

        if (requester is null || (!requester.IsAdmin && !requester.IsTeacherInGroup))
        {
            return [];
        }

        return await context.Groups
            .Where(g => g.GroupID == groupId)
            .SelectMany(g => g.Users)
            .Where(u => u.Roles.Any(r => r == Role.Student))
            .Select(u => u.UserID)
            .Distinct()
            .ToListAsync();
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