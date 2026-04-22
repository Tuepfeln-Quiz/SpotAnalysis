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
            Started = DateTime.UtcNow,
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
            attempt.Completed = DateTime.UtcNow;
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
    public async Task<List<DetailedQuizHistoryDto>> GetDetailedUserHistoryAsync(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var attempts = await context.QuizAttempts
            .Where(a => a.UserID == userId && a.Completed != DateTime.MinValue)
            .Include(a => a.STResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.STLResults)
            .Include(a => a.Quiz).ThenInclude(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
            .OrderByDescending(a => a.Started)
            .ToListAsync();
        
        var result = new List<DetailedQuizHistoryDto>();
        
        foreach (var a in attempts)
        {
            var dto = new DetailedQuizHistoryDto
            {
                AttemptId = a.AttemptID,
                QuizId = a.QuizID,
                QuizName = a.Quiz.Name,
                QuizType = DetermineQuizType(a),
                Started = a.Started,
                Completed = a.Completed == DateTime.MinValue ? null : a.Completed,
                CorrectAnswers = CalculateCorrect(a),
                TotalQuestions = CalculateTotal(a)
            };
            
            // Add question results
            foreach (var stl in a.STLResults)
            {
                dto.QuestionResults.Add(new QuestionResultDto
                {
                    QuestionId = stl.QuestionID,
                    QuestionTitle = "STL Question", // TODO: Get actual title if available
                    QuestionType = QuestionType.SpotTestLight,
                    TotalSubQuestions = 1,
                    CorrectSubQuestions = stl.IsCorrect ? 1 : 0
                });
            }
            
            foreach (var st in a.STResults)
            {
                dto.QuestionResults.Add(new QuestionResultDto
                {
                    QuestionId = st.QuestionID,
                    QuestionTitle = "ST Question", // TODO: Get actual title
                    QuestionType = QuestionType.SpotTest,
                    TotalSubQuestions = st.ChemicalResults.Count,
                    CorrectSubQuestions = st.ChemicalResults.Count(c => c.IsCorrect)
                });
            }
            
            result.Add(dto);
        }
        
        return result;
    }
    
    public async Task<GlobalStatisticsDto> GetGlobalStatisticsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var users = await context.Users.Include(u => u.Roles).ToListAsync();
        var totalUsers = users.Count;
        var totalTeachers = users.Count(u => u.Roles.Contains(Role.Teacher));
        var totalStudents = users.Count(u => u.Roles.Contains(Role.Student));
        
        var totalGroups = await context.Groups.CountAsync();
        var totalQuizzes = await context.Quizzes.CountAsync();
        
        var attempts = await context.QuizAttempts
            .Where(a => a.Completed != DateTime.MinValue)
            .Include(a => a.STResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.STLResults)
            .ToListAsync();
        
        var totalAttempts = attempts.Count;
        var totalCompletedAttempts = attempts.Count(a => a.Completed != DateTime.MinValue);
        
        var totalCorrect = 0;
        var totalQuestions = 0;
        
        foreach (var attempt in attempts)
        {
            totalCorrect += CalculateCorrect(attempt);
            totalQuestions += CalculateTotal(attempt);
        }
        
        return new GlobalStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalTeachers = totalTeachers,
            TotalStudents = totalStudents,
            TotalGroups = totalGroups,
            TotalQuizzes = totalQuizzes,
            TotalAttempts = totalAttempts,
            TotalCompletedAttempts = totalCompletedAttempts,
            AverageScorePercent = totalQuestions > 0 ? (totalCorrect * 100.0 / totalQuestions) : 0,
            TotalQuestionsAnswered = totalQuestions,
            TotalCorrectAnswers = totalCorrect
        };
    }
    
    public async Task<List<GroupStatisticsDto>> GetAllGroupStatisticsAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var groups = await context.Groups.Include(g => g.Users).ToListAsync();
        var result = new List<GroupStatisticsDto>();
        
        foreach (var group in groups)
        {
            var groupDto = await GetGroupStatisticsAsync(group.GroupID);
            result.Add(groupDto);
        }
        
        return result;
    }
    
    public async Task<GroupStatisticsDto> GetGroupStatisticsAsync(int groupId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var group = await context.Groups
            .Include(g => g.Users)
            .Include(g => g.Quizzes).ThenInclude(q => q.Attempts)
            .FirstOrDefaultAsync(g => g.GroupID == groupId);
        
        if (group == null) throw new ArgumentException("Group not found");
        
        var userIds = group.Users.Select(u => u.UserID).ToList();
        
        var attempts = await context.QuizAttempts
            .Where(a => userIds.Contains(a.UserID) && a.Completed != DateTime.MinValue)
            .Include(a => a.STResults).ThenInclude(r => r.ChemicalResults)
            .Include(a => a.STLResults)
            .Include(a => a.User)
            .ToListAsync();
        
        var totalCorrect = 0;
        var totalQuestions = 0;
        
        foreach (var attempt in attempts)
        {
            totalCorrect += CalculateCorrect(attempt);
            totalQuestions += CalculateTotal(attempt);
        }
        
        var userStats = new List<UserInGroupStatisticsDto>();
        foreach (var user in group.Users)
        {
            var userAttempts = attempts.Where(a => a.UserID == user.UserID).ToList();
            var userCorrect = userAttempts.Sum(a => CalculateCorrect(a));
            var userQuestions = userAttempts.Sum(a => CalculateTotal(a));
            
            userStats.Add(new UserInGroupStatisticsDto
            {
                UserId = user.UserID,
                UserName = user.UserName,
                TotalAttempts = userAttempts.Count,
                TotalCompletedAttempts = userAttempts.Count(a => a.Completed != DateTime.MinValue),
                AverageScorePercent = userQuestions > 0 ? (userCorrect * 100.0 / userQuestions) : 0
            });
        }
        
        return new GroupStatisticsDto
        {
            GroupId = group.GroupID,
            GroupName = group.Name,
            Description = group.Description,
            TotalUsers = group.Users.Count,
            TotalQuizzes = group.Quizzes.Count,
            TotalAttempts = attempts.Count,
            TotalCompletedAttempts = attempts.Count(a => a.Completed != DateTime.MinValue),
            AverageScorePercent = totalQuestions > 0 ? (totalCorrect * 100.0 / totalQuestions) : 0,
            UserStatistics = userStats
        };
    }
    
    private static int CalculateTotal(QuizAttempt attempt)
    {
        var total = 0;

        total += attempt.STLResults.Count;
        total += attempt.STResults.Sum(r => r.ChemicalResults.Count);

        return total;
    }
}