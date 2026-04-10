using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class QuizService(IDbContextFactory<AnalysisContext> factory) : IQuizService
{
    public async Task<List<QuizOverviewDto>> GetAllQuizzes()
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.Quizzes.Select(qu => new QuizOverviewDto
        {
            Id = qu.QuizID,
            Name = qu.Name,
            STCount = qu.Questions.Count(x => x.Type == QuestionType.SpotTest),
            STLCount = qu.Questions.Count(x => x.Type == QuestionType.SpotTestLight)
        }).ToListAsync();
    }

    public async Task CreateQuiz(Guid createdBy, ConfigQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        var newQuiz = new Quiz
        {
            Name = quiz.Name,
            CreatedBy = createdBy
        };

        await dbContext.Quizzes.AddAsync(newQuiz);
        await dbContext.SaveChangesAsync();

        var quizQuestions = quiz.Questions.Select(x => new QuizQuestion
        {
            QuizID = newQuiz.QuizID,
            QuestionID = x.Id,
            Order = x.Order
        });

        await dbContext.QuizQuestions.AddRangeAsync(quizQuestions);
        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public Task UpdateQuiz(ConfigQuizDto quiz)
    {
        throw new NotImplementedException();
    }

    public Task DeleteQuiz(int quizId)
    {
        throw new NotImplementedException();
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
                STLQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTestLight)
                    .Select(STLQuestionDto.FromQuestion).ToList(),
                STQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTest)
                    .Select(STQuestionDto.FromQuestion).ToList(),
            }).SingleAsync();
    }

    public async Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult)
    {
        throw new NotImplementedException();
    }

    public async Task<List<QuestionOverviewDto>> GetQuestions()
    {
        throw new NotImplementedException();
    }

    public async Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId)
    {
        throw new NotImplementedException();
    }

    public Task CreateSTQuestion(ConfigSTQuestionDto question)
    {
        throw new NotImplementedException();
    }

    public Task CreateSTLQuestion(ConfigSTLQuestionDto question)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSTQuestion(ConfigSTQuestionDto question)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSTLQuestion(ConfigSTLQuestionDto question)
    {
        throw new NotImplementedException();
    }

    public Task DeleteQuestion(int questionId)
    {
        throw new NotImplementedException();
    }
}