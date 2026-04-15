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
            STCount = qu.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTest),
            STLCount = qu.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTestLight),
            GroupCount = qu.Groups.Count
        }).ToListAsync();
    }

    public async Task CreateQuiz(Guid teacherId, CreateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        var newQuiz = new Quiz
        {
            Name = quiz.Name,
            CreatedBy = teacherId
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

    public async Task UpdateQuiz(Guid teacherId, UpdateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existingQuiz = await dbContext.Quizzes.SingleOrDefaultAsync(x => x.QuizID == quiz.Id);

        if (existingQuiz is null)
        {
            logger.LogError("Quiz with quiz id {quizId} does not exist.", quiz.Id);
            throw new KeyNotFoundException("The quiz requested quiz does not exist");
        }
        
        if (existingQuiz.CreatedBy != teacherId)
        {
            logger.LogError("A quiz can only be updated by its creator! Creator id: {creatorId}, Updater id: {updatedBy}", 
                existingQuiz.CreatedBy, teacherId);
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

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleAsync(q => q.QuizID == quizId);

        var group = await dbContext.Groups.SingleAsync(g => g.GroupID == groupId);

        if (quiz.Groups.Any(g => g.GroupID == groupId))
            return;

        quiz.Groups.Add(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveGroupFromQuiz(int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleAsync(q => q.QuizID == quizId);

        var group = quiz.Groups.FirstOrDefault(g => g.GroupID == groupId);
        if (group is null)
            return;

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
                STCount = q.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTest),
                STLCount = q.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTestLight),
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
                            Id = question.Question.STLQuestion.Reaction.Chemical1ID,
                            Color = question.Question.STLQuestion.Reaction.Chemical1.Color,
                            Name = question.Question.STLQuestion.Reaction.Chemical1.Name,
                            Formula = question.Question.STLQuestion.Reaction.Formula,
                            MethodInfo = question.Question.STLQuestion.Reaction.Chemical1.MethodOutputs.Select(mo => new MethodInfoDto
                            {
                                Name = mo.Method.Name,
                                Color = mo.Color,
                            }).ToList(),
                        },
                        Observation = question.Question.STLQuestion.Reaction.Observation.Description
                    }).ToList(),
                STQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTest)
                    .Select(question => new STQuestionDto
                    {
                        Id = question.QuestionID,
                        Description = question.Question.Description,
                        Order = question.Order,
                        Chemicals = question.Question.STQuestion.AvailableChemicals.Select(sta => new ChemicalQuestionDto
                        {
                            Id = sta.ChemicalID,
                            Color = sta.Chemical.Color,
                            Name = sta.Chemical.Name,
                            Formula = sta.Chemical.Formula,
                            IsAdditive = sta.Chemical.Type == ChemicalType.Additive,
                        }).ToList(),
                        Methods = question.Question.STQuestion.AvailableMethods.Select(am => new MethodQuestionDto
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
                StlAvailableReactions = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId).STLQuestion.AvailableReactions.ToList(),
                Question = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId),
                Attempt = x.Attempts.Single(a => a.UserID == answer.UserId),
                StlInput = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId).STLQuestion.Reaction
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
                StAvailableChemicals = x.Questions.Single(qq => qq.QuestionID == answer.QuestionId).STQuestion.AvailableChemicals.ToList()
            })
            .SingleAsync();
        
        var result = new STResult
        {
            QuestionID = quiz.Question.QuestionID,
            AttemptID = quiz.Attempt.AttemptID
        };

        context.STResults.Add(result);

        await context.SaveChangesAsync();

        var orderedAvailableChemicals = quiz.Question.STQuestion.AvailableChemicals.OrderBy(sta => sta.Order).ToList();

        var chemicalResults = answer.ChemicalFormulas
            .Select((_, i) => new STChemicalResult
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
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.Questions
            .AsNoTracking()
            .Select(q => q.Type == QuestionType.SpotTest ? new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Description = q.Description,
                Type = q.Type,
                CreatedByName = q.Creator.UserName,
                QuizCount = q.QuizQuestions.Count,
                ChemicalCount = q.STQuestion!.AvailableChemicals.Count,
                MethodCount = q.STQuestion.AvailableMethods.Count,

            } : new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Description = q.Description,
                Type = q.Type,
                CreatedByName = q.Creator.UserName,
                QuizCount = q.QuizQuestions.Count,
                ReactionCount = q.STLQuestion!.AvailableReactions.Count,
            }).ToListAsync();
    }

    public async Task<QuestionDetailDto> GetQuestionDetail(int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var qType = (await dbContext.Questions.AsNoTracking().SingleAsync(q => q.QuestionID == questionId))
            .Type;

        switch (qType)
        {
            case QuestionType.SpotTest:
                return await dbContext.Questions
                    .AsNoTracking()
                    .Where(x => x.QuestionID == questionId)
                    .Select(x => new QuestionDetailDto
                    {
                        Id = x.QuestionID,
                        Description = x.Description,
                        Type = x.Type,
                        Chemicals = x.STQuestion.AvailableChemicals.Select(ac => new ChemicalQuestionDto
                        {
                           Id = ac.ChemicalID,
                           Name = ac.Chemical.Name,
                           Color = ac.Chemical.Color,
                           Formula = ac.Chemical.Formula,
                           IsAdditive = ac.Chemical.Type == ChemicalType.Additive
                        }).ToList(),
                        Methods = x.STQuestion.AvailableMethods.Select(am => new MethodQuestionDto
                        {
                            Id = am.MethodID,
                            Name = am.Method.Name
                        }).ToList(),
                    }).SingleAsync();
                case QuestionType.SpotTestLight:
                    return await dbContext.Questions
                        .AsNoTracking()
                        .Where(x => x.QuestionID == questionId)
                        .Select(x => new QuestionDetailDto
                        {
                            Id = x.QuestionID,
                            Description = x.Description,
                            Type = x.Type,
                            AvailableReactionIds = x.STLQuestion.AvailableReactions
                                .Where(x => x.QuestionID == questionId)
                                .Select(x => x.ReactionID).ToList(),
                            ReactionId = x.STLQuestion.ReactionID,
                        }).SingleAsync();
                default:
                    throw new Exception("Invalid SpotTest question type");
        }
    }

    public async Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.QuizQuestions
            .AsNoTracking()
            .Where(qq => qq.QuizID == quizId)
            .OrderBy(qq => qq.Order)
            .Select(q => q.Question.Type == QuestionType.SpotTest ? new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Description = q.Question.Description,
                Type = q.Question.Type,
                CreatedByName = q.Question.Creator.UserName,
                QuizCount = q.Question.QuizQuestions.Count,
                ChemicalCount = q.Question.STQuestion!.AvailableChemicals.Count,
                MethodCount = q.Question.STQuestion.AvailableMethods.Count,

            } : new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Description = q.Question.Description,
                Type = q.Question.Type,
                CreatedByName = q.Question.Creator.UserName,
                QuizCount = q.Question.QuizQuestions.Count,
                ReactionCount = q.Question.STLQuestion!.AvailableReactions.Count,
            }).ToListAsync();
    }

    public async Task CreateSTQuestion(Guid teacherId, ConfigSTQuestionDto question)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var newQuestion = new Question
        {
            Description = question.Description,
            Type = QuestionType.SpotTest,
            CreatedBy = teacherId,
            Title = question.Title,
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

    public async Task CreateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var newQuestion = new Question
        {
            Description = question.Description,
            Type = QuestionType.SpotTestLight,
            CreatedBy = teacherId,
            Title = question.Title
        };

        await dbContext.Questions.AddAsync(newQuestion);
        await dbContext.SaveChangesAsync();

        var stlQuestion = new STLQuestion
        {
            QuestionID = newQuestion.QuestionID,
            ReactionID = question.ReactionId,
            ShownEductID = question.ShowEductId,
        };
        
        await dbContext.STLQuestions.AddAsync(stlQuestion);
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

    public async Task UpdateSTQuestion(Guid teacherId, ConfigSTQuestionDto question)
    {
        if (question.Id is null)
            throw new ArgumentException("Question Id is required for update.");

        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.Questions.SingleOrDefaultAsync(q => q.QuestionID == question.Id);
        if (existing is null)
            throw new KeyNotFoundException($"Question with id {question.Id} not found.");

        existing.Description = question.Description;

        await dbContext.STAvailableChemicals
            .Where(c => c.QuestionID == question.Id)
            .ExecuteDeleteAsync();

        var chemicals = question.AvailableChemicals.Select((chemId, index) => new STAvailableChemical
        {
            QuestionID = question.Id.Value,
            ChemicalID = chemId,
            Order = index
        });
        await dbContext.STAvailableChemicals.AddRangeAsync(chemicals);

        await dbContext.STAvailableMethods
            .Where(m => m.QuestionID == question.Id)
            .ExecuteDeleteAsync();

        var methods = question.AvailableMethods.Select(methodId => new STAvailableMethod
        {
            QuestionID = question.Id.Value,
            MethodID = methodId
        });
        await dbContext.STAvailableMethods.AddRangeAsync(methods);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task UpdateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question)
    {
        if (question.Id is null)
            throw new ArgumentException("Question Id is required for update.");

        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.Questions
            .Include(q => q.STLQuestion)
            .SingleOrDefaultAsync(q => q.QuestionID == question.Id && q.Type == QuestionType.SpotTestLight);
        
        if (existing?.STLQuestion is null)
            throw new KeyNotFoundException($"Question with id {question.Id} not found.");

        existing.Description = question.Description;
        existing.STLQuestion.ReactionID = question.ReactionId;

        await dbContext.STLAvailableReactions
            .Where(r => r.QuestionID == question.Id)
            .ExecuteDeleteAsync();

        var reactions = question.AvailableReactions.Select(reactionId => new STLAvailableReaction
        {
            QuestionID = question.Id.Value,
            ReactionID = reactionId
        });
        await dbContext.STLAvailableReactions.AddRangeAsync(reactions);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    
    public async Task DeleteQuestion(int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var question = await dbContext.Questions.SingleAsync(x => x.QuestionID == questionId);

        switch (question.Type)
        {
            case QuestionType.SpotTest:
                await dbContext.STAvailableChemicals.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STAvailableMethods.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STResults.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                var chemicalResult = await dbContext.STResults.SingleAsync(x => x.QuestionID == questionId);
                await dbContext.STChemicalResults.Where(x => x.ResultID == chemicalResult.ResultID).ExecuteDeleteAsync();
                dbContext.STResults.Remove(chemicalResult);
                await dbContext.SaveChangesAsync();
                break;
            case QuestionType.SpotTestLight:
                await dbContext.STLAvailableReactions.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STLResults.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                break;
        }

        await dbContext.Questions.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
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