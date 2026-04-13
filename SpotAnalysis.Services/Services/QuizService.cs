using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;
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

    public async Task<STLResult> ValidateAndSaveStlQuestion(ValidateStlQuestionDto answer)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var quiz = await context.Users.Where(u => u.UserID == answer.UserId).SelectMany(u => u.Quizzes)
            .Where(q => q.QuizID == answer.QuizId)
            .Select(x => new StlQuestionData
            {
                StlAvailableReactions = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId).STLAvailableReactions.ToList(),
                Question = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId),
                Attempt = x.Attempts.Single(a => a.UserID == answer.UserId),
                StlInput = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId).STLInput
            })
            .SingleAsync();

        var reaction = await context.Reactions.Where(r => r.ReactionID == answer.ReactionId)
            .Include(reaction => reaction.Observation)
            .SingleAsync();
                
        var newResult = new STLResult
        {
            AttemptID = quiz.Attempt.AttemptID,
            QuestionID = quiz.Question.QuestionID,
            ChosenReactionID = reaction.ReactionID,
            IsCorrect = quiz.StlInput!.Observation == reaction.Observation
        };
        
        await context.STLResults.AddAsync(newResult);

        return newResult;
    }

    public async Task<STResult> ValidateAndSaveStQuestion(ValidateStQuestionDto answer)
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.Database.BeginTransactionAsync();
        
        var quiz = await context.Users.Where(u => u.UserID == answer.UserId).SelectMany(u => u.Quizzes)
            .Where(q => q.QuizID == answer.QuizId)
            .Select(x => new StQuestionData
            {
                Attempt = x.Attempts.Single(a => a.UserID == answer.UserId),
                Question = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId),
                StAvailableChemicals = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId).STAvailableChemicals.ToList()
            })
            .SingleAsync();
        
        var result = new STResult
        {
            QuestionID = quiz.Question.QuestionID,
            AttemptID = quiz.Attempt.AttemptID,
        };

        context.STResults.Add(result);

        await context.SaveChangesAsync();

        var orderedAvailableChemicals = quiz.Question.STAvailableChemicals.OrderBy(sta => sta.Order).ToList();

        var chemicalResults = answer.ChemicalFormulas
            .Select((t, i) => new STChemicalResult
            {
                ResultID = result.ResultID, 
                ChosenFormula = answer.ChemicalFormulas.ElementAt(i), 
                IsCorrect = orderedAvailableChemicals.ElementAt(i).Chemical.Formula == answer.ChemicalFormulas.ElementAt(i),
            }).ToList();

        await context.STChemicalResults.AddRangeAsync(chemicalResults);

        await context.SaveChangesAsync();

        await context.Database.CommitTransactionAsync();

        return result;
    }

    public async Task<List<QuestionOverviewDto>> GetQuestions()
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.Questions
            .AsNoTracking()
            .Select(q => new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Description = q.Description,
                Type = q.Type,
                CreatedByName = q.Creator.UserName,
                ChemicalCount = q.STAvailableChemicals.Count,
                MethodCount = q.STAvailableMethods.Count,
                ReactionCount = q.STLAvailableReactions.Count,
                QuizCount = q.QuizQuestions.Count
            }).ToListAsync();
    }

    public async Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.QuizQuestions
            .AsNoTracking()
            .Where(qq => qq.QuizID == quizId)
            .OrderBy(qq => qq.Order)
            .Select(qq => new QuestionOverviewDto
            {
                Id = qq.Question.QuestionID,
                Description = qq.Question.Description,
                Type = qq.Question.Type,
                CreatedByName = qq.Question.Creator.UserName,
                ChemicalCount = qq.Question.STAvailableChemicals.Count,
                MethodCount = qq.Question.STAvailableMethods.Count,
                ReactionCount = qq.Question.STLAvailableReactions.Count,
                QuizCount = qq.Question.QuizQuestions.Count
            }).ToListAsync();
    }

    public async Task CreateSTQuestion(Guid createdBy, ConfigSTQuestionDto question)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var newQuestion = new Question
        {
            Description = question.Description,
            Type = QuestionType.SpotTest,
            CreatedBy = createdBy,
            ReactionID = null
        };

        await dbContext.Questions.AddAsync(newQuestion);
        await dbContext.SaveChangesAsync();

        var chemicals = question.AvailableChemicals.Select((chemId, index) => new STAvailableChemical
        {
            QuestionID = newQuestion.QuestionID,
            ChemicalID = chemId,
            Order = index
        });
        await dbContext.STAvailableChemicals.AddRangeAsync(chemicals);

        var methods = question.AvailableMethods.Select(methodId => new STAvailableMethod
        {
            QuestionID = newQuestion.QuestionID,
            MethodID = methodId
        });
        await dbContext.STAvailableMethods.AddRangeAsync(methods);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task CreateSTLQuestion(Guid createdBy, ConfigSTLQuestionDto question)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var newQuestion = new Question
        {
            Description = question.Description,
            Type = QuestionType.SpotTestLight,
            CreatedBy = createdBy,
            ReactionID = question.ReactionId
        };

        await dbContext.Questions.AddAsync(newQuestion);
        await dbContext.SaveChangesAsync();

        var reactions = question.AvailableReactions.Select(reactionId => new STLAvailableReaction
        {
            QuestionID = newQuestion.QuestionID,
            ReactionID = reactionId
        });
        await dbContext.STLAvailableReactions.AddRangeAsync(reactions);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
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

    private class StlQuestionData
    {
        public List<STLAvailableReaction> StlAvailableReactions { get; set; } = [];
        public required Question Question { get; set; }
        public required QuizAttempt Attempt  { get; set; }
        public Reaction? StlInput { get; set; }
    }

    private class StQuestionData
    {
        public required QuizAttempt Attempt  { get; set; }
        public required Question Question { get; set; }
        public List<STAvailableChemical> StAvailableChemicals { get; set; } = [];
        public List<STAvailableMethod> StAvailableMethods { get; set; } = [];
    }
}