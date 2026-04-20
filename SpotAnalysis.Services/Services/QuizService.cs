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

    public async Task<int> CreateQuiz(Guid teacherId, CreateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var newQuiz = new Quiz
        {
            Name = quiz.Name,
            CreatedBy = teacherId
        };

        dbContext.Quizzes.Add(newQuiz);

        var quizQuestions = quiz.Questions.Select(x => new QuizQuestion
        {
            Quiz = newQuiz,
            QuestionID = x.Id,
            Order = x.Order
        });

        dbContext.QuizQuestions.AddRange(quizQuestions);
        await dbContext.SaveChangesAsync();

        return newQuiz.QuizID;
    }

    public async Task UpdateQuiz(Guid teacherId, UpdateQuizDto quiz)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existingQuiz = await dbContext.Quizzes.SingleOrDefaultAsync(x => x.QuizID == quiz.Id);

        if (existingQuiz is null)
        {
            logger.LogError("Quiz with quiz id {quizId} does not exist.", quiz.Id);
            throw new KeyNotFoundException("The requested quiz does not exist");
        }

        if (existingQuiz.CreatedBy != teacherId)
        {
            logger.LogError("A quiz can only be updated by its creator! Creator id: {creatorId}, Updater id: {updatedBy}",
                existingQuiz.CreatedBy, teacherId);
            throw new UnauthorizedAccessException("A quiz can only be updated by its creator");
        }

        existingQuiz.Name = quiz.Name;

        var incomingQuestionIds = quiz.Questions.Select(x => x.Id).ToHashSet();

        var existingQuizQuestions = await dbContext.QuizQuestions
            .Where(x => x.QuizID == quiz.Id)
            .ToListAsync();

        var existingQuestionIds = existingQuizQuestions.Select(x => x.QuestionID).ToHashSet();

        // Update order on questions that are retained
        var orderLookup = quiz.Questions.ToDictionary(q => q.Id, q => q.Order);
        foreach (var existing in existingQuizQuestions.Where(eq => incomingQuestionIds.Contains(eq.QuestionID)))
        {
            existing.Order = orderLookup[existing.QuestionID];
        }

        // Add new questions
        var newQuestions = quiz.Questions.Where(q => !existingQuestionIds.Contains(q.Id));
        await dbContext.QuizQuestions.AddRangeAsync(newQuestions.Select(x => new QuizQuestion
        {
            QuizID = quiz.Id,
            QuestionID = x.Id,
            Order = x.Order
        }));

        // Remove deleted questions
        var questionsToDelete = existingQuizQuestions
            .Where(eq => !incomingQuestionIds.Contains(eq.QuestionID))
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
            .SingleOrDefaultAsync(q => q.QuizID == quizId && q.CreatedBy == teacherId);

        if (quiz is null) return;

        quiz.Groups.Clear();

        dbContext.QuizQuestions.RemoveRange(
            await dbContext.QuizQuestions.Where(x => x.QuizID == quizId).ToListAsync());

        dbContext.Quizzes.Remove(quiz);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task AssignGroupToQuiz(Guid teacherId, int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleAsync(q => q.QuizID == quizId);

        if (quiz.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("Only the quiz creator can manage group assignments.");

        var group = await dbContext.Groups.SingleAsync(g => g.GroupID == groupId);

        if (quiz.Groups.Any(g => g.GroupID == groupId))
            return;

        quiz.Groups.Add(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<GroupDto>> GetGroupsByQuiz(Guid teacherId, int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes.AsNoTracking().SingleAsync(q => q.QuizID == quizId);
        if (quiz.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("Only the quiz creator can view group assignments.");

        return await dbContext.Quizzes
            .Where(q => q.QuizID == quizId)
            .SelectMany(q => q.Groups)
            .Select(g => new GroupDto
            {
                Id = g.GroupID,
                Name = g.Name,
                Description = g.Description,
            }).ToListAsync();
    }

    public async Task RemoveGroupFromQuiz(Guid teacherId, int quizId, int groupId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleAsync(q => q.QuizID == quizId);

        if (quiz.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("Only the quiz creator can manage group assignments.");

        var group = quiz.Groups.FirstOrDefault(g => g.GroupID == groupId);
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
                        q.Groups.Any(g => g.Users.Any(u => u.UserID == studentId)))
            .Select(q => new
            {
                Quiz = q,
                LatestAttempt = q.Attempts
                    .Where(a => a.UserID == studentId)
                    .OrderByDescending(a => a.AttemptID)
                    .FirstOrDefault()
            })
            .Select(x => new QuizOverviewDto
            {
                Id = x.Quiz.QuizID,
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

    public async Task<STLResult> ValidateAndSaveStlQuestion(ValidateStlQuestionDto answer)
    {
        await using var context = await factory.CreateDbContextAsync();

        var attempt = await GetOpenAttempt(context, answer.UserId, answer.QuizId);

        var correctObservationId = await context.Questions
            .Where(q => q.QuestionID == answer.QuestionId)
            .Select(q => q.STLQuestion!.Reaction.ObservationID)
            .SingleAsync();

        var chosenObservationId = await context.Reactions
            .Where(r => r.ReactionID == answer.ReactionId)
            .Select(r => r.ObservationID)
            .SingleAsync();

        var newResult = new STLResult
        {
            AttemptID = attempt.AttemptID,
            QuestionID = answer.QuestionId,
            ChosenReactionID = answer.ReactionId,
            IsCorrect = correctObservationId == chosenObservationId
        };

        await context.STLResults.AddAsync(newResult);
        await context.SaveChangesAsync();

        return newResult;
    }

    public async Task<STResult> ValidateAndSaveStQuestion(ValidateStQuestionDto answer)
    {
        await using var context = await factory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var attempt = await GetOpenAttempt(context, answer.UserId, answer.QuizId);

        var orderedEducts = await context.Questions
            .Where(q => q.QuestionID == answer.QuestionId)
            .SelectMany(q => q.STQuestion!.AvailableChemicals)
            .Where(ac => ac.Chemical.Type == ChemicalType.Educt)
            .OrderBy(ac => ac.Order)
            .Select(ac => new { ac.ChemicalID, ac.Chemical.Formula })
            .ToListAsync();

        if (answer.ChemicalFormulas.Count != orderedEducts.Count)
            throw new ArgumentException(
                $"Expected {orderedEducts.Count} formulas but received {answer.ChemicalFormulas.Count}.");

        var result = new STResult
        {
            QuestionID = answer.QuestionId,
            AttemptID = attempt.AttemptID
        };
        context.STResults.Add(result);
        await context.SaveChangesAsync();

        var chemicalResults = answer.ChemicalFormulas
            .Select((formula, i) => new STChemicalResult
            {
                ResultID = result.ResultID,
                ChemicalID = orderedEducts[i].ChemicalID,
                ChosenFormula = formula,
                IsCorrect = orderedEducts[i].Formula == formula
            }).ToList();

        await context.STChemicalResults.AddRangeAsync(chemicalResults);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return result;
    }

    private static async Task<QuizAttempt> GetOpenAttempt(AnalysisContext context, Guid userId, int quizId)
    {
        var attempt = await context.QuizAttempts
            .Where(a => a.UserID == userId && a.QuizID == quizId && a.Completed == null)
            .OrderByDescending(a => a.AttemptID)
            .FirstOrDefaultAsync();

        if (attempt is null)
            throw new InvalidOperationException(
                $"No open attempt for user {userId} on quiz {quizId}.");

        return attempt;
    }

    public async Task<List<QuestionOverviewDto>> GetQuestions()
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        return await dbContext.Questions
            .AsNoTracking()
            .Select(q => new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Title = q.Title,
                Description = q.Description,
                Type = q.Type,
                CreatedById = q.CreatedBy,
                CreatedByName = q.Creator.UserName,
                QuizCount = q.QuizQuestions.Count,
                ChemicalCount = q.STQuestion != null ? q.STQuestion.AvailableChemicals.Count : 0,
                MethodCount = q.STQuestion != null ? q.STQuestion.AvailableMethods.Count : 0,
                ReactionCount = q.STLQuestion != null ? q.STLQuestion.AvailableReactions.Count : 0,
            })
            .ToListAsync();
    }

    public async Task<QuestionDetailDto> GetQuestionDetail(int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var question = await dbContext.Questions
            .AsNoTracking()
            .Where(q => q.QuestionID == questionId)
            .Select(x => new
            {
                x.QuestionID,
                x.Title,
                x.Description,
                x.Type,
                Chemicals = x.STQuestion != null
                    ? x.STQuestion.AvailableChemicals.Select(ac => new ChemicalQuestionDto
                    {
                        Id = ac.ChemicalID,
                        Name = ac.Chemical.Name,
                        Color = ac.Chemical.Color,
                        Formula = ac.Chemical.Formula,
                        IsAdditive = ac.Chemical.Type == ChemicalType.Additive
                    }).ToList()
                    : new List<ChemicalQuestionDto>(),
                Methods = x.STQuestion != null
                    ? x.STQuestion.AvailableMethods.Select(am => new MethodQuestionDto
                    {
                        Id = am.MethodID,
                        Name = am.Method.Name
                    }).ToList()
                    : new List<MethodQuestionDto>(),
                AvailableReactionIds = x.STLQuestion != null
                    ? x.STLQuestion.AvailableReactions.Select(ar => ar.ReactionID).ToList()
                    : new List<int>(),
                ReactionId = x.STLQuestion != null ? x.STLQuestion.ReactionID : 0,
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
            .Where(qq => qq.QuizID == quizId)
            .OrderBy(qq => qq.Order)
            .Select(qq => new QuestionOverviewDto
            {
                Id = qq.Question.QuestionID,
                Title = qq.Question.Title,
                Description = qq.Question.Description,
                Type = qq.Question.Type,
                CreatedById = qq.Question.CreatedBy,
                CreatedByName = qq.Question.Creator.UserName,
                QuizCount = qq.Question.QuizQuestions.Count,
                ChemicalCount = qq.Question.STQuestion != null ? qq.Question.STQuestion.AvailableChemicals.Count : 0,
                MethodCount = qq.Question.STQuestion != null ? qq.Question.STQuestion.AvailableMethods.Count : 0,
                ReactionCount = qq.Question.STLQuestion != null ? qq.Question.STLQuestion.AvailableReactions.Count : 0,
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

        var stQuestion = new STQuestion { Question = newQuestion };
        dbContext.STQuestions.Add(stQuestion);

        var chemicals = question.AvailableChemicals.Select((chemId, index) => new STAvailableChemical
        {
            STQuestion = stQuestion,
            ChemicalID = chemId,
            Order = index
        });
        dbContext.STAvailableChemicals.AddRange(chemicals);

        var methods = question.AvailableMethods.Select(methodId => new STAvailableMethod
        {
            STQuestion = stQuestion,
            MethodID = methodId
        });
        dbContext.STAvailableMethods.AddRange(methods);

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

        var stlQuestion = new STLQuestion
        {
            Question = newQuestion,
            ReactionID = question.ReactionId,
            ShownEductID = question.ShowEductId,
        };

        dbContext.STLQuestions.Add(stlQuestion);

        var reactions = question.AvailableReactions.Select(reactionId => new STLAvailableReaction
        {
            STLQuestion = stlQuestion,
            ReactionID = reactionId
        });

        dbContext.STLAvailableReactions.AddRange(reactions);

        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateSTQuestion(Guid teacherId, ConfigSTQuestionDto question)
    {
        if (question.Id is null)
            throw new ArgumentException("Question Id is required for update.");

        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var existing = await dbContext.Questions
            .Include(q => q.STQuestion)
            .SingleOrDefaultAsync(q => q.QuestionID == question.Id && q.Type == QuestionType.SpotTest);

        if (existing?.STQuestion is null)
            throw new KeyNotFoundException($"SpotTest question with id {question.Id} not found.");

        if (existing.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("A question can only be updated by its creator.");

        existing.Title = question.Title;
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

        if (existing.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("A question can only be updated by its creator.");

        existing.Title = question.Title;
        existing.Description = question.Description;
        existing.STLQuestion.ReactionID = question.ReactionId;
        existing.STLQuestion.ShownEductID = question.ShowEductId;

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

    public async Task DeleteQuestion(Guid teacherId, int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var question = await dbContext.Questions.SingleAsync(x => x.QuestionID == questionId);

        if (question.CreatedBy != teacherId)
            throw new UnauthorizedAccessException("A question can only be deleted by its creator.");

        if (await dbContext.QuizQuestions.AnyAsync(x => x.QuestionID == questionId))
            throw new InvalidOperationException(
                $"Question {questionId} is used in one or more quizzes and cannot be deleted.");

        switch (question.Type)
        {
            case QuestionType.SpotTest:
                var resultIds = await dbContext.STResults
                    .Where(x => x.QuestionID == questionId)
                    .Select(x => x.ResultID)
                    .ToListAsync();
                await dbContext.STChemicalResults.Where(x => resultIds.Contains(x.ResultID)).ExecuteDeleteAsync();
                await dbContext.STResults.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STAvailableChemicals.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STAvailableMethods.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STQuestions.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                break;
            case QuestionType.SpotTestLight:
                await dbContext.STLResults.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STLAvailableReactions.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                await dbContext.STLQuestions.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();
                break;
        }

        await dbContext.Questions.Where(x => x.QuestionID == questionId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task<QuizPlayDto> StartOrResumeQuiz(Guid userId, int quizId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var hasAccess = await db.Quizzes.AnyAsync(q =>
            q.QuizID == quizId &&
            (q.CreatedBy == userId || q.Groups.Any(g => g.Users.Any(u => u.UserID == userId))));
        if (!hasAccess)
            throw new UnauthorizedAccessException("User has no access to this quiz.");

        var openAttempt = await db.QuizAttempts
            .Where(a => a.UserID == userId && a.QuizID == quizId && a.Completed == null)
            .OrderByDescending(a => a.AttemptID)
            .FirstOrDefaultAsync();

        if (openAttempt is null)
        {
            openAttempt = new QuizAttempt
            {
                UserID = userId,
                QuizID = quizId,
                Started = DateTime.UtcNow
            };
            db.QuizAttempts.Add(openAttempt);
            await db.SaveChangesAsync();
        }

        return await BuildQuizPlayDto(db, quizId, openAttempt.AttemptID);
    }

    private static async Task<QuizPlayDto> BuildQuizPlayDto(AnalysisContext db, int quizId, int attemptId)
    {
        var methods = await db.Methods.AsNoTracking()
            .ToDictionaryAsync(m => m.MethodID, m => m.Name);

        var quiz = await db.Quizzes
            .AsNoTracking()
            .Where(q => q.QuizID == quizId)
            .Select(q => new
            {
                q.QuizID,
                q.Name,
                Questions = q.QuizQuestions.OrderBy(qq => qq.Order).Select(qq => new
                {
                    qq.QuestionID,
                    qq.Order,
                    qq.Question.Description,
                    qq.Question.Type,
                    SpotTest = qq.Question.STQuestion != null ? new
                    {
                        UnknownEducts = qq.Question.STQuestion.AvailableChemicals
                            .Where(ac => ac.Chemical.Type == ChemicalType.Educt)
                            .OrderBy(ac => ac.Order)
                            .Select(ac => new
                            {
                                ac.Chemical.ChemicalID,
                                ac.Chemical.Name,
                                ac.Chemical.Formula,
                                ac.Chemical.ImagePath,
                                ac.Chemical.Type,
                                ac.Chemical.Color,
                                MethodOutputs = ac.Chemical.MethodOutputs.Select(mo => new { mo.MethodID, mo.Color }).ToList()
                            }).ToList(),
                        AvailableAdditives = qq.Question.STQuestion.AvailableChemicals
                            .Where(ac => ac.Chemical.Type == ChemicalType.Additive)
                            .OrderBy(ac => ac.Order)
                            .Select(ac => new
                            {
                                ac.Chemical.ChemicalID,
                                ac.Chemical.Name,
                                ac.Chemical.Formula,
                                ac.Chemical.ImagePath,
                                ac.Chemical.Type,
                                ac.Chemical.Color,
                                MethodOutputs = ac.Chemical.MethodOutputs.Select(mo => new { mo.MethodID, mo.Color }).ToList()
                            }).ToList(),
                        AvailableMethods = qq.Question.STQuestion.AvailableMethods
                            .Select(am => am.MethodID).ToList()
                    } : null,
                    Light = qq.Question.STLQuestion != null ? new
                    {
                        ShownEductId = qq.Question.STLQuestion.ShownEduct.ChemicalID,
                        ShownEductName = qq.Question.STLQuestion.ShownEduct.Name,
                        ShownEductFormula = qq.Question.STLQuestion.ShownEduct.Formula,
                        ShownEductColor = qq.Question.STLQuestion.ShownEduct.Color,
                        ShownEductMethodOutputs = qq.Question.STLQuestion.ShownEduct.MethodOutputs
                            .Select(mo => new { mo.MethodID, mo.Color }).ToList(),
                        Observation = qq.Question.STLQuestion.Reaction.Observation.Description,
                        CorrectReactionID = qq.Question.STLQuestion.ReactionID,
                        AvailableReactions = qq.Question.STLQuestion.AvailableReactions.Select(ar => new
                        {
                            ar.Reaction.ReactionID,
                            ar.Reaction.Chemical1ID,
                            ar.Reaction.Chemical2ID,
                            Chemical1Name = ar.Reaction.Chemical1.Name,
                            Chemical2Name = ar.Reaction.Chemical2.Name,
                            ar.Reaction.RelevantProduct,
                            ar.Reaction.Formula,
                            ObservationDescription = ar.Reaction.Observation.Description,
                            ar.Reaction.ImagePath
                        }).ToList()
                    } : null
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
                SpotTest = q.SpotTest != null ? new SpotTestPayloadDto
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
                        Color = e.Color,
                        MethodOutputs = e.MethodOutputs.ToDictionary(mo => methods[mo.MethodID], mo => mo.Color)
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
                        Color = e.Color,
                        MethodOutputs = e.MethodOutputs.ToDictionary(mo => methods[mo.MethodID], mo => mo.Color)
                    }).ToList(),
                    AvailableMethods = q.SpotTest.AvailableMethods.Select(id => methods[id]).ToList()
                } : null,
                Light = q.Light != null ? new LightPayloadDto
                {
                    ShownEduct = new ChemicalDto
                    {
                        Id = q.Light.ShownEductId,
                        Name = q.Light.ShownEductName,
                        Formula = q.Light.ShownEductFormula,
                        Color = q.Light.ShownEductColor,
                        MethodInfo = q.Light.ShownEductMethodOutputs.Select(mo => new MethodInfoDto
                        {
                            Name = methods[mo.MethodID],
                            Color = mo.Color
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
                } : null
            }).ToList()
        };
    }

    public async Task<QuizPlayDto> StartNewAttempt(Guid userId, int quizId)
    {
        await using var db = await factory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var hasAccess = await db.Quizzes.AnyAsync(q =>
            q.QuizID == quizId &&
            (q.CreatedBy == userId || q.Groups.Any(g => g.Users.Any(u => u.UserID == userId))));
        if (!hasAccess)
            throw new UnauthorizedAccessException("User has no access to this quiz.");

        var openAttempt = await db.QuizAttempts
            .Where(a => a.UserID == userId && a.QuizID == quizId && a.Completed == null)
            .OrderByDescending(a => a.AttemptID)
            .FirstOrDefaultAsync();
        if (openAttempt is not null)
        {
            openAttempt.Completed = DateTime.UtcNow;
        }

        var fresh = new QuizAttempt
        {
            UserID = userId,
            QuizID = quizId,
            Started = DateTime.UtcNow
        };
        db.QuizAttempts.Add(fresh);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await BuildQuizPlayDto(db, quizId, fresh.AttemptID);
    }

    public async Task CompleteAttempt(Guid userId, int attemptId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var attempt = await db.QuizAttempts
            .SingleOrDefaultAsync(a => a.AttemptID == attemptId && a.UserID == userId);

        if (attempt is null)
            throw new UnauthorizedAccessException("Attempt does not belong to the requesting user.");

        attempt.Completed = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
