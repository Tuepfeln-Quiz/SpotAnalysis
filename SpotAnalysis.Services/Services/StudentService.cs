using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class StudentService(IDbContextFactory<AnalysisContext> factory) : IStudentService
{
    public async Task Register(string userName, string password, string? email)
    {
        var userId = Guid.NewGuid();
        var passwordString = new PasswordProvider.Password(password, userId).ParamString();
        
        await using var context = await factory.CreateDbContextAsync();
        var newUser = new User
        {
            UserName = userName,
            PasswordHash = passwordString,
            UserID = userId
        };
        newUser.Roles.Add(await context.Roles.SingleAsync(r => r.Title == "student"));
        context.Users.Add(newUser);
        
        await context.SaveChangesAsync();
    }

    public async Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return context.Users.Where(u => u.UserID == studentId)
            .SelectMany(u => u.Groups)
            .SelectMany(g => g.Quizzes)
            .Select(q => new QuizOverviewDto
            {
                Id = q.QuizID,
                Name = q.Name,
                STCount = q.Questions.Select(qq => qq.Type == QuestionType.SpotTest).Count(),
                STLCount = q.Questions.Select(qq => qq.Type == QuestionType.SpotTestLight).Count(),
            })
            .ToList();
    }

    public async Task<QuizDto> OpenQuiz(Guid studentId, int quizId)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = await context.QuizAttempts.Where(qa => qa.UserID == studentId && qa.QuizID == quizId)
            .FirstOrDefaultAsync();

        if (attempt == null)
        {
            context.QuizAttempts.Add(new QuizAttempt
            {
                QuizID = quizId,
                UserID = studentId,
                Started = DateTime.Now
            });
        }
        
        return await context.Quizzes.Where(q => q.QuizID == quizId)
            .Select(q => new QuizDto
            {
                Name = q.Name,
                STLQuestions = q.Questions.Where(qq => qq.Type == QuestionType.SpotTestLight).Select(STLQuestionDto.FromQuestion).ToList(),
                STQuestions = q.Questions.Where(qq => qq.Type == QuestionType.SpotTest).Select(STQuestionDto.FromQuestion).ToList(),
            }).SingleAsync();
    }

    public async Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult)
    {
        throw new NotImplementedException();
    }
}