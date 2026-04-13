using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class QuizService(ILogger<QuizService> logger, IDbContextFactory<AnalysisContext> factory) : IQuizService
{
    public async Task<List<QuizOverviewDto>> GetAllQuizzes()
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.Quizzes
            .AsNoTracking()
            .Select(qu => new QuizOverviewDto
        {
            Id = qu.QuizID,
            Name = qu.Name,
            STCount = qu.Questions.Count(x => x.Type == QuestionType.SpotTest),
            STLCount = qu.Questions.Count(x => x.Type == QuestionType.SpotTestLight)
        }).ToListAsync();
    }

    public async Task CreateQuiz(Guid createdBy, CreateQuizDto quiz)
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

    public async Task UpdateQuiz(Guid updatedBy, UpdateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existingQuiz = await dbContext.Quizzes.SingleOrDefaultAsync(x => x.QuizID == quiz.Id);

        if (existingQuiz is null)
        {
            logger.LogError("Quiz with quiz id {quizId} does not exist.", quiz.Id);
            throw new KeyNotFoundException("The quiz requested quiz does not exist");
        }
        
        if (existingQuiz.CreatedBy != updatedBy)
        {
            logger.LogError("A quiz can only be updated by its creator! Creator id: {creatorId}, Updator id: {updatedBy}", 
                existingQuiz.CreatedBy, updatedBy);
            
            // throw new Exception
            return;
        }

        existingQuiz.Name = quiz.Name;

        var questionIds = quiz.Questions.Select(x => x.Id).ToArray();

        var existingQuestionIds = await dbContext.QuizQuestions
            .AsNoTracking()
            .Where(x => x.QuizID == quiz.Id)
            .Select(x => x.QuestionID)
            .ToListAsync();

        var newQuestions = quiz.Questions.ExceptBy(existingQuestionIds, question => question.Id).ToArray();
        await dbContext.QuizQuestions.AddRangeAsync(newQuestions.Select(x => new QuizQuestion
        {
            QuizID = quiz.Id,
            QuestionID = x.Id,
            Order = x.Order
        }));
        
        var questionsToDelete = existingQuestionIds.Except(questionIds).ToList();
        await dbContext.QuizQuestions.Where(x => questionsToDelete.Contains(x.QuestionID)).ExecuteDeleteAsync();
        
        await dbContext.SaveChangesAsync();
        
        await transaction.CommitAsync();
    }

    public async Task DeleteQuiz(int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        await dbContext.QuizQuestions
            .Where(x => x.QuizID == quizId)
            .ExecuteDeleteAsync();

        await dbContext.Quizzes
            .Where(x => x.QuizID == quizId)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public Task AssignGroupToQuiz(int quizId, int groupId)
    {
        // var group = 
        throw new NotImplementedException();
    }

    public Task RemoveGroupToQuiz(int quizId, int groupId)
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