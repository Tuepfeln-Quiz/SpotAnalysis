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
            throw new UnauthorizedAccessException("A quiz can only be updated by its creator");
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

    public async Task DeleteQuiz(Guid teacherId, int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        await dbContext.QuizQuestions
            .Where(x => x.QuizID == quizId && x.Quiz.CreatedBy == teacherId)
            .ExecuteDeleteAsync();

        await dbContext.Quizzes
            .Where(x => x.QuizID == quizId && x.CreatedBy == teacherId)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task AssignGroupToQuiz(int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var group = await dbContext.Groups.SingleAsync(x => x.GroupID == groupId);

        var quiz = await dbContext.Quizzes.Include(x => x.Groups).SingleAsync(x => x.QuizID == quizId);
        
        quiz.Groups.Add(group);

        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveGroupFromQuiz(int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var group = await dbContext.Groups.SingleAsync(x => x.GroupID == groupId);

        var quiz = await dbContext.Quizzes.Include(x => x.Groups).SingleAsync(x => x.QuizID == quizId);
        
        quiz.Groups.Remove(group);

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return context.Quizzes
            .Where(q => q.CreatedBy == studentId || 
                        q.Groups.Any(g => 
                            g.Users.Any(u => u.UserID == studentId)))
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

        await context.SaveChangesAsync();

        return await context.Quizzes.Where(q => q.QuizID == quizId)
            .Select(q => new QuizDto
            {
                Name = q.Name,
                STLQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTestLight)
                    .Select(question => new STLQuestionDto
                    {
                        Id = question.QuestionID,
                        Description = question.Question.Description,
                        Order = question.Order,
                        Educt = new ChemicalDto
                        {
                            Id = question.Question.STLInput.Chemical1ID,
                            Color = question.Question.STLInput.Chemical1.Color,
                            Name = question.Question.STLInput.Chemical1.Name,
                            Formula = question.Question.STLInput.Formula,
                            MethodInfo = question.Question.STLInput.Chemical1.MethodOutputs.Select(mo => new MethodInfoDto
                            {
                                Name = mo.Method.Name,
                                Color = mo.Color,
                            }).ToList(),
                        },
                        Observation = question.Question.STLInput.Observation.Description
                    }).ToList(),
                STQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTest)
                    .Select(question => new STQuestionDto
                    {
                        Id = question.QuestionID,
                        Description = question.Question.Description,
                        Order = question.Order,
                        Chemicals = question.Question.STAvailableChemicals.Select(sta => new ChemicalQuestionDto
                        {
                            Id = sta.ChemicalID,
                            Color = sta.Chemical.Color,
                            Name = sta.Chemical.Name,
                            Formula = sta.Chemical.Formula,
                            IsAdditive = sta.Chemical.Type == ChemicalType.Additive,
                        }).ToList(),
                        Methods = question.Question.STAvailableMethods.Select(am => new MethodQuestionDto
                        {
                            Name = am.Method.Name,
                            Id = am.MethodID,
                        }).ToList(),
                    }).ToList(),
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

    public async Task<QuizAttempt?> GetQuizAttempt(Guid studentId, int quizId)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.QuizAttempts.Where(qa => qa.QuizID == quizId && qa.UserID == studentId).FirstOrDefaultAsync();
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