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
                Id = qu.QuizId,
                Name = qu.Name,
                STCount = qu.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTest),
                STLCount = qu.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTestLight),
                GroupCount = qu.Groups.Count
            }).ToListAsync();
    }

    public async Task<int> CreateQuiz(Guid teacherId, CreateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var newQuiz = new Quiz { Name = quiz.Name, CreatedBy = teacherId };

        dbContext.Quizzes.Add(newQuiz);

        var quizQuestions = quiz.Questions.Select(x => new QuizQuestion
        {
            Quiz = newQuiz, QuestionId = x.Id, Order = x.Order
        });

        dbContext.QuizQuestions.AddRange(quizQuestions);
        await dbContext.SaveChangesAsync();

        return newQuiz.QuizId;
    }

    public async Task UpdateQuiz(Guid teacherId, UpdateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existingQuiz = await dbContext.Quizzes.SingleOrDefaultAsync(x => x.QuizId == quiz.Id);

        if (existingQuiz is null)
        {
            logger.LogError("Quiz with quiz id {quizId} does not exist.", quiz.Id);
            throw new KeyNotFoundException("The requested quiz does not exist");
        }

        if (existingQuiz.CreatedBy != teacherId)
        {
            logger.LogError(
                "A quiz can only be updated by its creator! Creator id: {creatorId}, Updater id: {updatedBy}",
                existingQuiz.CreatedBy, teacherId);
            throw new UnauthorizedAccessException("A quiz can only be updated by its creator");
        }

        existingQuiz.Name = quiz.Name;

        var incomingQuestionIds = quiz.Questions.Select(x => x.Id).ToHashSet();

        var existingQuizQuestions = await dbContext.QuizQuestions
            .Where(x => x.QuizId == quiz.Id)
            .ToListAsync();

        var existingQuestionIds = existingQuizQuestions.Select(x => x.QuestionId).ToHashSet();

        // Update order on questions that are retained
        var orderLookup = quiz.Questions.ToDictionary(q => q.Id, q => q.Order);
        foreach (var existing in existingQuizQuestions.Where(eq => incomingQuestionIds.Contains(eq.QuestionId)))
        {
            existing.Order = orderLookup[existing.QuestionId];
        }

        // Add new questions
        var newQuestions = quiz.Questions.Where(q => !existingQuestionIds.Contains(q.Id));
        await dbContext.QuizQuestions.AddRangeAsync(newQuestions.Select(x => new QuizQuestion
        {
            QuizId = quiz.Id, QuestionId = x.Id, Order = x.Order
        }));

        // Remove deleted questions
        var questionsToDelete = existingQuizQuestions
            .Where(eq => !incomingQuestionIds.Contains(eq.QuestionId))
            .ToList();
        dbContext.QuizQuestions.RemoveRange(questionsToDelete);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task DeleteQuiz(Guid teacherId, int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleOrDefaultAsync(q => q.QuizId == quizId && q.CreatedBy == teacherId);

        if (quiz is null)
            return;

        quiz.Groups.Clear();

        await dbContext.QuizAttempts.Where(x => x.QuizId == quiz.QuizId).ExecuteDeleteAsync();

        await dbContext.QuizQuestions.Where(x => x.QuizId == quizId).ExecuteDeleteAsync();

        dbContext.Quizzes.Remove(quiz);
        await dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task AssignGroupToQuiz(Guid teacherId, int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleAsync(q => q.QuizId == quizId);

        if (quiz.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("Only the quiz creator can manage group assignments.");

        var group = await dbContext.Groups.SingleAsync(g => g.GroupId == groupId);

        if (quiz.Groups.Any(g => g.GroupId == groupId))
            return;

        quiz.Groups.Add(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<GroupDto>> GetGroupsByQuiz(Guid teacherId, int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes.AsNoTracking().SingleAsync(q => q.QuizId == quizId);
        if (quiz.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("Only the quiz creator can view group assignments.");

        return await dbContext.Quizzes
            .Where(q => q.QuizId == quizId)
            .SelectMany(q => q.Groups)
            .Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, Description = g.Description, }).ToListAsync();
    }

    public async Task RemoveGroupFromQuiz(Guid teacherId, int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleAsync(q => q.QuizId == quizId);

        if (quiz.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("Only the quiz creator can manage group assignments.");

        var group = quiz.Groups.FirstOrDefault(g => g.GroupId == groupId);
        if (group is null)
            return;

        quiz.Groups.Remove(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.Quizzes
            .Where(q => q.CreatedBy == studentId ||
                        q.Groups.Any(g => g.Users.Any(u => u.UserId == studentId)))
            .Select(q => new
            {
                Quiz = q,
                LatestAttempt = q.Attempts
                    .Where(a => a.UserId == studentId)
                    .OrderByDescending(a => a.AttemptId)
                    .FirstOrDefault()
            })
            .Select(x => new QuizOverviewDto
            {
                Id = x.Quiz.QuizId,
                Name = x.Quiz.Name,
                STCount = x.Quiz.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTest),
                STLCount = x.Quiz.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTestLight),
                QuestionCount = x.Quiz.QuizQuestions.Count,
                GroupCount = x.Quiz.Groups.Count,
                LastAttemptStatus =
                    x.LatestAttempt == null
                        ? LastAttemptStatus.NotStarted
                        : x.LatestAttempt.Completed == null
                            ? LastAttemptStatus.InProgress
                            : LastAttemptStatus.Completed,
                LastCompletedAt = x.LatestAttempt != null ? x.LatestAttempt.Completed : null
            })
            .ToListAsync();
    }

    public async Task<StlResult> ValidateAndSaveStlQuestion(ValidateStlQuestionDto answer)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = await GetOpenAttempt(context, answer.UserId, answer.QuizId);

        var correctObservationId = await context.Questions
            .Where(q => q.QuestionId == answer.QuestionId)
            .Select(q => q.StlQuestion!.Reaction.ObservationId)
            .SingleAsync();

        var chosenObservationId = await context.Reactions
            .Where(r => r.ReactionId == answer.ReactionId)
            .Select(r => r.ObservationId)
            .SingleAsync();

        var newResult = new StlResult
        {
            AttemptId = attempt.AttemptId,
            QuestionId = answer.QuestionId,
            ChosenReactionId = answer.ReactionId,
            IsCorrect = correctObservationId == chosenObservationId
        };

        await context.StlResults.AddAsync(newResult);
        await context.SaveChangesAsync();

        return newResult;
    }

    public async Task<StResult> ValidateAndSaveStQuestion(ValidateStQuestionDto answer)
    {
        await using var context = await factory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var attempt = await GetOpenAttempt(context, answer.UserId, answer.QuizId);

        var orderedEducts = await context.Questions
            .Where(q => q.QuestionId == answer.QuestionId)
            .SelectMany(q => q.StQuestion!.AvailableChemicals)
            .Where(ac => ac.Chemical.Type == ChemicalType.Educt)
            .OrderBy(ac => ac.Order)
            .Select(ac => new { ChemicalID = ac.ChemicalId, ac.Chemical.Formula })
            .ToListAsync();

        if (answer.ChemicalFormulas.Count != orderedEducts.Count)
            throw new ArgumentException(
                $"Expected {orderedEducts.Count} formulas but received {answer.ChemicalFormulas.Count}.");

        var result = new StResult { QuestionId = answer.QuestionId, AttemptId = attempt.AttemptId };
        context.StResults.Add(result);
        await context.SaveChangesAsync();

        var chemicalResults = answer.ChemicalFormulas
            .Select((formula, i) => new StChemicalResult
            {
                ResultId = result.ResultId,
                ChemicalId = orderedEducts[i].ChemicalID,
                ChosenFormula = formula,
                IsCorrect = orderedEducts[i].Formula == formula
            }).ToList();

        await context.StChemicalResults.AddRangeAsync(chemicalResults);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return result;
    }

    public async Task<List<QuestionOverviewDto>> GetQuestions()
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.Questions
            .AsNoTracking()
            .Select(q => new QuestionOverviewDto
            {
                Id = q.QuestionId,
                Title = q.Title,
                Description = q.Description,
                Type = q.Type,
                CreatedById = q.CreatedBy,
                CreatedByName = q.Creator.UserName,
                QuizCount = q.QuizQuestions.Count,
                ChemicalCount = q.StQuestion != null ? q.StQuestion.AvailableChemicals.Count : 0,
                MethodCount = q.StQuestion != null ? q.StQuestion.AvailableMethods.Count : 0,
                ReactionCount = q.StlQuestion != null ? q.StlQuestion.AvailableReactions.Count : 0,
            })
            .ToListAsync();
    }

    public async Task<QuestionDetailDto> GetQuestionDetail(int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var question = await dbContext.Questions
            .AsNoTracking()
            .Where(q => q.QuestionId == questionId)
            .Select(x => new
            {
                QuestionID = x.QuestionId,
                x.Title,
                x.Description,
                x.Type,
                Chemicals = x.StQuestion != null
                    ? x.StQuestion.AvailableChemicals.Select(ac => new ChemicalQuestionDto
                    {
                        Id = ac.ChemicalId,
                        Name = ac.Chemical.Name,
                        Color = new ColorDto
                        {
                            Id = ac.Chemical.Color.ColorId,
                            Name = ac.Chemical.Color.Name,
                            HexValue = ac.Chemical.Color.HexValue
                        },
                        Formula = ac.Chemical.Formula,
                        IsAdditive = ac.Chemical.Type == ChemicalType.Additive
                    }).ToList()
                    : new List<ChemicalQuestionDto>(),
                Methods = x.StQuestion != null
                    ? x.StQuestion.AvailableMethods.Select(am => new MethodQuestionDto
                    {
                        Id = am.MethodId, Name = am.Method.Name
                    }).ToList()
                    : new List<MethodQuestionDto>(),
                AvailableReactionIds = x.StlQuestion != null
                    ? x.StlQuestion.AvailableReactions.Select(ar => ar.ReactionId).ToList()
                    : new List<int>(),
                ReactionId = x.StlQuestion != null ? x.StlQuestion.ReactionId : 0,
            })
            .SingleAsync();

        return new QuestionDetailDto
        {
            Id = question.QuestionID,
            Title = question.Title,
            Description = question.Description,
            Type = question.Type,
            Chemicals = question.Chemicals,
            Methods = question.Methods,
            AvailableReactionIds = question.AvailableReactionIds,
            ReactionId = question.ReactionId,
        };
    }

    public async Task<List<QuestionOverviewDto>> GetQuestionsOfQuiz(int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.QuizQuestions
            .AsNoTracking()
            .Where(qq => qq.QuizId == quizId)
            .OrderBy(qq => qq.Order)
            .Select(qq => new QuestionOverviewDto
            {
                Id = qq.Question.QuestionId,
                Title = qq.Question.Title,
                Description = qq.Question.Description,
                Type = qq.Question.Type,
                CreatedById = qq.Question.CreatedBy,
                CreatedByName = qq.Question.Creator.UserName,
                QuizCount = qq.Question.QuizQuestions.Count,
                ChemicalCount = qq.Question.StQuestion != null ? qq.Question.StQuestion.AvailableChemicals.Count : 0,
                MethodCount = qq.Question.StQuestion != null ? qq.Question.StQuestion.AvailableMethods.Count : 0,
                ReactionCount = qq.Question.StlQuestion != null ? qq.Question.StlQuestion.AvailableReactions.Count : 0,
            })
            .ToListAsync();
    }

    public async Task CreateSTQuestion(Guid teacherId, ConfigSTQuestionDto question)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var newQuestion = new Question
        {
            Description = question.Description,
            Type = QuestionType.SpotTest,
            CreatedBy = teacherId,
            Title = question.Title,
        };

        dbContext.Questions.Add(newQuestion);

        var stQuestion = new StQuestion { Question = newQuestion };
        dbContext.StQuestions.Add(stQuestion);

        var chemicals = question.AvailableChemicals.Select((chemId, index) => new StAvailableChemical
        {
            StQuestion = stQuestion, ChemicalId = chemId, Order = index
        });
        dbContext.StAvailableChemicals.AddRange(chemicals);

        var methods = question.AvailableMethods.Select(methodId => new StAvailableMethod
        {
            StQuestion = stQuestion, MethodId = methodId
        });
        dbContext.StAvailableMethods.AddRange(methods);

        await dbContext.SaveChangesAsync();
    }

    public async Task CreateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var newQuestion = new Question
        {
            Description = question.Description,
            Type = QuestionType.SpotTestLight,
            CreatedBy = teacherId,
            Title = question.Title
        };

        dbContext.Questions.Add(newQuestion);

        var stlQuestion = new StlQuestion
        {
            Question = newQuestion, ReactionId = question.ReactionId, ShownEductId = question.ShowEductId,
        };

        dbContext.StlQuestions.Add(stlQuestion);

        var reactions = question.AvailableReactions.Select(reactionId => new StlAvailableReaction
        {
            StlQuestion = stlQuestion, ReactionId = reactionId
        });

        dbContext.StlAvailableReactions.AddRange(reactions);

        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateSTQuestion(Guid teacherId, ConfigSTQuestionDto question)
    {
        if (question.Id is null)
            throw new ArgumentException("Question Id is required for update.");

        var questionId = question.Id.Value;

        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.Questions
            .Include(q => q.StQuestion)
            .SingleOrDefaultAsync(q => q.QuestionId == question.Id && q.Type == QuestionType.SpotTest);

        if (existing?.StQuestion is null)
            throw new KeyNotFoundException($"SpotTest question with id {question.Id} not found.");

        if (existing.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("A question can only be updated by its creator.");

        existing.Title = question.Title;
        existing.Description = question.Description;

        await dbContext.StAvailableChemicals
            .Where(c => c.QuestionId == question.Id)
            .ExecuteDeleteAsync();

        var chemicals = question.AvailableChemicals.Select((chemId, index) => new StAvailableChemical
        {
            QuestionId = questionId, ChemicalId = chemId, Order = index
        });
        await dbContext.StAvailableChemicals.AddRangeAsync(chemicals);

        await dbContext.StAvailableMethods
            .Where(m => m.QuestionId == question.Id)
            .ExecuteDeleteAsync();

        var methods = question.AvailableMethods.Select(methodId => new StAvailableMethod
        {
            QuestionId = questionId, MethodId = methodId
        });
        await dbContext.StAvailableMethods.AddRangeAsync(methods);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task UpdateSTLQuestion(Guid teacherId, ConfigSTLQuestionDto question)
    {
        if (question.Id is null)
            throw new ArgumentException("Question Id is required for update.");

        var questionId = question.Id.Value;

        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.Questions
            .Include(q => q.StlQuestion)
            .SingleOrDefaultAsync(q => q.QuestionId == question.Id && q.Type == QuestionType.SpotTestLight);

        if (existing?.StlQuestion is null)
            throw new KeyNotFoundException($"Question with id {question.Id} not found.");

        if (existing.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("A question can only be updated by its creator.");

        existing.Title = question.Title;
        existing.Description = question.Description;
        existing.StlQuestion.ReactionId = question.ReactionId;
        existing.StlQuestion.ShownEductId = question.ShowEductId;

        await dbContext.StlAvailableReactions
            .Where(r => r.QuestionId == question.Id)
            .ExecuteDeleteAsync();

        var reactions = question.AvailableReactions.Select(reactionId => new StlAvailableReaction
        {
            QuestionId = questionId, ReactionId = reactionId
        });
        await dbContext.StlAvailableReactions.AddRangeAsync(reactions);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task DeleteQuestion(Guid teacherId, int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var question = await dbContext.Questions.SingleAsync(x => x.QuestionId == questionId);

        if (question.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("A question can only be deleted by its creator.");

        if (await dbContext.QuizQuestions.AnyAsync(x => x.QuestionId == questionId))
            throw new InvalidOperationException(
                $"Question {questionId} is used in one or more quizzes and cannot be deleted.");

        switch (question.Type)
        {
            case QuestionType.SpotTest:
                var resultIds = await dbContext.StResults
                    .Where(x => x.QuestionId == questionId)
                    .Select(x => x.ResultId)
                    .ToListAsync();
                await dbContext.StChemicalResults.Where(x => resultIds.Contains(x.ResultId)).ExecuteDeleteAsync();
                await dbContext.StResults.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                await dbContext.StAvailableChemicals.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                await dbContext.StAvailableMethods.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                await dbContext.StQuestions.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                break;
            case QuestionType.SpotTestLight:
                await dbContext.StlResults.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                await dbContext.StlAvailableReactions.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                await dbContext.StlQuestions.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();
                break;
        }

        await dbContext.Questions.Where(x => x.QuestionId == questionId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task<QuizPlayDto> StartOrResumeQuiz(Guid userId, int quizId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var hasAccess = await db.Quizzes.AnyAsync(q =>
            q.QuizId == quizId &&
            (q.CreatedBy == userId || q.Groups.Any(g => g.Users.Any(u => u.UserId == userId))));
        if (!hasAccess)
            throw new UnauthorizedAccessException("User has no access to this quiz.");

        var openAttempt = await db.QuizAttempts
            .Where(a => a.UserId == userId && a.QuizId == quizId && a.Completed == null)
            .OrderByDescending(a => a.AttemptId)
            .FirstOrDefaultAsync();

        if (openAttempt is null)
        {
            openAttempt = new QuizAttempt { UserId = userId, QuizId = quizId, Started = DateTime.UtcNow };
            db.QuizAttempts.Add(openAttempt);
            await db.SaveChangesAsync();
        }

        return await BuildQuizPlayDto(db, quizId, openAttempt.AttemptId);
    }

    public async Task<QuizPlayDto> StartNewAttempt(Guid userId, int quizId)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var hasAccess = await db.Quizzes.AnyAsync(q =>
            q.QuizId == quizId &&
            (q.CreatedBy == userId || q.Groups.Any(g => g.Users.Any(u => u.UserId == userId))));
        if (!hasAccess)
            throw new UnauthorizedAccessException("User has no access to this quiz.");

        var openAttempt = await db.QuizAttempts
            .Where(a => a.UserId == userId && a.QuizId == quizId && a.Completed == null)
            .OrderByDescending(a => a.AttemptId)
            .FirstOrDefaultAsync();
        openAttempt?.Completed = DateTime.UtcNow;

        var fresh = new QuizAttempt { UserId = userId, QuizId = quizId, Started = DateTime.UtcNow };
        db.QuizAttempts.Add(fresh);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await BuildQuizPlayDto(db, quizId, fresh.AttemptId);
    }

    public async Task CompleteAttempt(Guid userId, int attemptId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var attempt = await db.QuizAttempts
                          .SingleOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId) ??
                      throw new UnauthorizedAccessException("Attempt does not belong to the requesting user.");

        attempt.Completed = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task<QuizAttempt> GetOpenAttempt(AnalysisContext context, Guid userId, int quizId)
    {
        var attempt = await context.QuizAttempts
            .Where(a => a.UserId == userId && a.QuizId == quizId && a.Completed == null)
            .OrderByDescending(a => a.AttemptId)
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException(
            $"No open attempt for user {userId} on quiz {quizId}.");

        return attempt;
    }

    private static async Task<QuizPlayDto> BuildQuizPlayDto(AnalysisContext db, int quizId, int attemptId)
    {
        var methods = await db.Methods.AsNoTracking()
            .ToDictionaryAsync(m => m.MethodId, m => m.Name);

        var quiz = await db.Quizzes
            .AsNoTracking()
            .Where(q => q.QuizId == quizId)
            .Select(q => new
            {
                QuizID = q.QuizId,
                q.Name,
                Questions = q.QuizQuestions.OrderBy(qq => qq.Order).Select(qq => new
                {
                    QuestionID = qq.QuestionId,
                    qq.Order,
                    qq.Question.Description,
                    qq.Question.Type,
                    SpotTest = qq.Question.StQuestion != null
                        ? new
                        {
                            UnknownEducts = qq.Question.StQuestion.AvailableChemicals
                                .Where(ac => ac.Chemical.Type == ChemicalType.Educt)
                                .OrderBy(ac => ac.Order)
                                .Select(ac => new
                                {
                                    ChemicalID = ac.Chemical.ChemicalId,
                                    ac.Chemical.Name,
                                    ac.Chemical.Formula,
                                    ac.Chemical.ImagePath,
                                    ac.Chemical.Type,
                                    Color = new
                                    {
                                        ac.Chemical.Color.ColorId, ac.Chemical.Color.Name, ac.Chemical.Color.HexValue
                                    },
                                    MethodOutputs = ac.Chemical.MethodOutputs
                                        .Select(mo => new
                                        {
                                            MethodID = mo.MethodId,
                                            Color = new { mo.Color.ColorId, mo.Color.Name, mo.Color.HexValue }
                                        }).ToList()
                                }).ToList(),
                            AvailableAdditives = qq.Question.StQuestion.AvailableChemicals
                                .Where(ac => ac.Chemical.Type == ChemicalType.Additive)
                                .OrderBy(ac => ac.Order)
                                .Select(ac => new
                                {
                                    ChemicalID = ac.Chemical.ChemicalId,
                                    ac.Chemical.Name,
                                    ac.Chemical.Formula,
                                    ac.Chemical.ImagePath,
                                    ac.Chemical.Type,
                                    Color = new
                                    {
                                        ac.Chemical.Color.ColorId, ac.Chemical.Color.Name, ac.Chemical.Color.HexValue
                                    },
                                    MethodOutputs = ac.Chemical.MethodOutputs
                                        .Select(mo => new
                                        {
                                            MethodID = mo.MethodId,
                                            Color = new { mo.Color.ColorId, mo.Color.Name, mo.Color.HexValue }
                                        }).ToList()
                                }).ToList(),
                            AvailableMethods = qq.Question.StQuestion.AvailableMethods
                                .Select(am => am.MethodId).ToList()
                        }
                        : null,
                    Light = qq.Question.StlQuestion != null
                        ? new
                        {
                            ShownEductId = qq.Question.StlQuestion.ShownEduct.ChemicalId,
                            ShownEductName = qq.Question.StlQuestion.ShownEduct.Name,
                            ShownEductFormula = qq.Question.StlQuestion.ShownEduct.Formula,
                            ShownEductColor = new
                            {
                                qq.Question.StlQuestion.ShownEduct.Color.ColorId,
                                qq.Question.StlQuestion.ShownEduct.Color.Name,
                                qq.Question.StlQuestion.ShownEduct.Color.HexValue
                            },
                            ShownEductMethodOutputs = qq.Question.StlQuestion.ShownEduct.MethodOutputs
                                .Select(mo => new
                                {
                                    MethodID = mo.MethodId,
                                    Color = new { mo.Color.ColorId, mo.Color.Name, mo.Color.HexValue }
                                })
                                .ToList(),
                            Observation = qq.Question.StlQuestion.Reaction.Observation.Description,
                            CorrectReactionID = qq.Question.StlQuestion.ReactionId,
                            AvailableReactions = qq.Question.StlQuestion.AvailableReactions.Select(ar => new
                            {
                                ReactionID = ar.Reaction.ReactionId,
                                Chemical1ID = ar.Reaction.Chemical1Id,
                                Chemical2ID = ar.Reaction.Chemical2Id,
                                Chemical1Name = ar.Reaction.Chemical1.Name,
                                Chemical2Name = ar.Reaction.Chemical2.Name,
                                ar.Reaction.RelevantProduct,
                                ar.Reaction.Formula,
                                ObservationDescription = ar.Reaction.Observation.Description,
                                ar.Reaction.ImagePath
                            }).ToList()
                        }
                        : null
                }).ToList()
            })
            .SingleAsync();

        return new QuizPlayDto
        {
            QuizID = quiz.QuizID,
            Name = quiz.Name,
            AttemptID = attemptId,
            Questions = quiz.Questions.Select(q => new QuizQuestionPayloadDto
            {
                QuestionID = q.QuestionID,
                Order = q.Order,
                Description = q.Description,
                Type = q.Type,
                SpotTest = q.SpotTest != null
                    ? new SpotTestPayloadDto
                    {
                        UnknownEducts = q.SpotTest.UnknownEducts.Select(e => new LabChemicalDto
                        {
                            ChemicalID = e.ChemicalID,
                            Name = e.Name,
                            Formula = e.Formula,
                            ImagePath = e.ImagePath,
                            Type = e.Type,
                            ChemicalTypeID = (int)e.Type,
                            ChemicalTypeName = "Edukt",
                            Color = new ColorDto
                            {
                                Id = e.Color.ColorId, Name = e.Color.Name, HexValue = e.Color.HexValue
                            },
                            MethodOutputs = e.MethodOutputs.ToDictionary(mo => methods[mo.MethodID],
                                mo => new ColorDto
                                {
                                    Id = mo.Color.ColorId, Name = mo.Color.Name, HexValue = mo.Color.HexValue
                                })
                        }).ToList(),
                        AvailableAdditives = q.SpotTest.AvailableAdditives.Select(e => new LabChemicalDto
                        {
                            ChemicalID = e.ChemicalID,
                            Name = e.Name,
                            Formula = e.Formula,
                            ImagePath = e.ImagePath,
                            Type = e.Type,
                            ChemicalTypeID = (int)e.Type,
                            ChemicalTypeName = "Zusatzstoff",
                            Color = new ColorDto
                            {
                                Id = e.Color.ColorId, Name = e.Color.Name, HexValue = e.Color.HexValue
                            },
                            MethodOutputs = e.MethodOutputs.ToDictionary(mo => methods[mo.MethodID],
                                mo => new ColorDto
                                {
                                    Id = mo.Color.ColorId, Name = mo.Color.Name, HexValue = mo.Color.HexValue
                                })
                        }).ToList(),
                        AvailableMethods = q.SpotTest.AvailableMethods.Select(id => methods[id]).ToList()
                    }
                    : null,
                Light = q.Light != null
                    ? new LightPayloadDto
                    {
                        ShownEduct =
                            new ChemicalDto
                            {
                                Id = q.Light.ShownEductId,
                                Name = q.Light.ShownEductName,
                                Formula = q.Light.ShownEductFormula,
                                Color =
                                    new ColorDto
                                    {
                                        Id = q.Light.ShownEductColor.ColorId,
                                        Name = q.Light.ShownEductColor.Name,
                                        HexValue = q.Light.ShownEductColor.HexValue
                                    },
                                MethodInfo =
                                    q.Light.ShownEductMethodOutputs.Select(mo =>
                                        new MethodInfoDto
                                        {
                                            Name = methods[mo.MethodID],
                                            Color =
                                                new ColorDto
                                                {
                                                    Id = mo.Color.ColorId,
                                                    Name = mo.Color.Name,
                                                    HexValue = mo.Color.HexValue
                                                }
                                        }).ToList()
                            },
                        Observation = q.Light.Observation,
                        CorrectReactionID = q.Light.CorrectReactionID,
                        AvailableReactions = q.Light.AvailableReactions.Select(ar => new LabReactionDto
                        {
                            ReactionID = ar.ReactionID,
                            Chemical1ID = ar.Chemical1ID,
                            Chemical2ID = ar.Chemical2ID,
                            Chemical1Name = ar.Chemical1Name,
                            Chemical2Name = ar.Chemical2Name,
                            RelevantProduct = ar.RelevantProduct,
                            Formula = ar.Formula,
                            ObservationDescription = ar.ObservationDescription,
                            ImagePath = ar.ImagePath
                        }).ToList()
                    }
                    : null
            }).ToList()
        };
    }
}
