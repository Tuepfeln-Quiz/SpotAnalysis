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
        await dbContext.QuizQuestions
            .Where(x => x.QuizID == quiz.Id && questionsToDelete.Contains(x.QuestionID))
            .ExecuteDeleteAsync();
        
        await dbContext.SaveChangesAsync();
        
        await transaction.CommitAsync();
    }

    public async Task DeleteQuiz(Guid teacherId, int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes
            .Include(q => q.Groups)
            .SingleOrDefaultAsync(q => q.QuizID == quizId && q.CreatedBy == teacherId);

        if (quiz is null) return;

        quiz.Groups.Clear();

        await dbContext.QuizQuestions
            .Where(x => x.QuizID == quizId)
            .ExecuteDeleteAsync();

        dbContext.Quizzes.Remove(quiz);
        await dbContext.SaveChangesAsync();
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

    public async Task<List<GroupDto>> GetGroupsByQuiz(int quizId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();

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
                        : x.LatestAttempt.Completed == default
                            ? LastAttemptStatus.InProgress
                            : LastAttemptStatus.Completed,
                LastCompletedAt =
                    x.LatestAttempt != null && x.LatestAttempt.Completed != default
                        ? x.LatestAttempt.Completed
                        : (DateTime?)null
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

        var result = new STResult
        {
            QuestionID = answer.QuestionId,
            AttemptID = attempt.AttemptID
        };
        context.STResults.Add(result);
        await context.SaveChangesAsync();

        var chemicalResults = answer.ChemicalFormulas
            .Take(orderedEducts.Count)
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
            .Where(a => a.UserID == userId && a.QuizID == quizId && a.Completed == default)
            .OrderByDescending(a => a.AttemptID)
            .FirstOrDefaultAsync();

        if (attempt is null)
            throw new InvalidOperationException(
                $"No open attempt for user {userId} on quiz {quizId}.");

        return attempt;
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
                Title = q.Title,
                Description = q.Description,
                Type = q.Type,
                CreatedById = q.CreatedBy,
                CreatedByName = q.Creator.UserName,
                QuizCount = q.QuizQuestions.Count,
                ChemicalCount = q.STQuestion!.AvailableChemicals.Count,
                MethodCount = q.STQuestion.AvailableMethods.Count,

            } : new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Title = q.Title,
                Description = q.Description,
                Type = q.Type,
                CreatedById = q.CreatedBy,
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
                        Title = x.Title,
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
                            Title = x.Title,
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
                Title = q.Question.Title,
                Description = q.Question.Description,
                Type = q.Question.Type,
                CreatedById = q.Question.CreatedBy,
                CreatedByName = q.Question.Creator.UserName,
                QuizCount = q.Question.QuizQuestions.Count,
                ChemicalCount = q.Question.STQuestion!.AvailableChemicals.Count,
                MethodCount = q.Question.STQuestion.AvailableMethods.Count,

            } : new QuestionOverviewDto
            {
                Id = q.QuestionID,
                Title = q.Question.Title,
                Description = q.Question.Description,
                Type = q.Question.Type,
                CreatedById = q.Question.CreatedBy,
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

        await dbContext.STQuestions.AddAsync(new STQuestion { QuestionID = newQuestion.QuestionID });
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

        var existing = await dbContext.Questions
            .Include(q => q.STQuestion)
            .SingleOrDefaultAsync(q => q.QuestionID == question.Id && q.Type == QuestionType.SpotTest);

        if (existing?.STQuestion is null)
            throw new KeyNotFoundException($"SpotTest question with id {question.Id} not found.");

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
    
    public async Task DeleteQuestion(int questionId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var question = await dbContext.Questions.SingleAsync(x => x.QuestionID == questionId);

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
            .Where(a => a.UserID == userId && a.QuizID == quizId && a.Completed == default)
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
        var quiz = await db.Quizzes
            .Where(q => q.QuizID == quizId)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STQuestion!)
                    .ThenInclude(st => st.AvailableChemicals)
                        .ThenInclude(ac => ac.Chemical)
                            .ThenInclude(c => c.MethodOutputs).ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STQuestion!)
                    .ThenInclude(st => st.AvailableMethods).ThenInclude(am => am.Method)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STLQuestion!)
                    .ThenInclude(stl => stl.ShownEduct)
                        .ThenInclude(c => c.MethodOutputs).ThenInclude(mo => mo.Method)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STLQuestion!)
                    .ThenInclude(stl => stl.Reaction).ThenInclude(r => r.Observation)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STLQuestion!)
                    .ThenInclude(stl => stl.AvailableReactions).ThenInclude(ar => ar.Reaction).ThenInclude(r => r.Chemical1)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STLQuestion!)
                    .ThenInclude(stl => stl.AvailableReactions).ThenInclude(ar => ar.Reaction).ThenInclude(r => r.Chemical2)
            .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .ThenInclude(question => question.STLQuestion!)
                    .ThenInclude(stl => stl.AvailableReactions).ThenInclude(ar => ar.Reaction).ThenInclude(r => r.Observation)
            .AsNoTracking()
            .SingleAsync();

        var questions = quiz.QuizQuestions
            .OrderBy(qq => qq.Order)
            .Select(qq => MapQuestionPayload(qq))
            .ToList();

        return new QuizPlayDto
        {
            QuizID = quiz.QuizID,
            Name = quiz.Name,
            AttemptID = attemptId,
            Questions = questions
        };
    }

    private static QuizQuestionPayloadDto MapQuestionPayload(QuizQuestion qq) => qq.Question.Type switch
    {
        QuestionType.SpotTest => new QuizQuestionPayloadDto
        {
            QuestionID = qq.QuestionID,
            Order = qq.Order,
            Description = qq.Question.Description,
            Type = QuestionType.SpotTest,
            SpotTest = new SpotTestPayloadDto
            {
                UnknownEducts = qq.Question.STQuestion!.AvailableChemicals
                    .Where(ac => ac.Chemical.Type == ChemicalType.Educt)
                    .OrderBy(ac => ac.Order)
                    .Select(ac => MapLabChemical(ac.Chemical)).ToList(),
                AvailableAdditives = qq.Question.STQuestion.AvailableChemicals
                    .Where(ac => ac.Chemical.Type == ChemicalType.Additive)
                    .OrderBy(ac => ac.Order)
                    .Select(ac => MapLabChemical(ac.Chemical)).ToList(),
                AvailableMethods = qq.Question.STQuestion.AvailableMethods
                    .Select(am => am.Method.Name).ToList()
            }
        },
        QuestionType.SpotTestLight => new QuizQuestionPayloadDto
        {
            QuestionID = qq.QuestionID,
            Order = qq.Order,
            Description = qq.Question.Description,
            Type = QuestionType.SpotTestLight,
            Light = new LightPayloadDto
            {
                ShownEduct = MapChemicalToChemicalDto(qq.Question.STLQuestion!.ShownEduct),
                Observation = qq.Question.STLQuestion.Reaction.Observation.Description,
                CorrectReactionID = qq.Question.STLQuestion.ReactionID,
                AvailableReactions = qq.Question.STLQuestion.AvailableReactions
                    .Select(ar => new LabReactionDto
                    {
                        ReactionID = ar.Reaction.ReactionID,
                        Chemical1ID = ar.Reaction.Chemical1ID,
                        Chemical2ID = ar.Reaction.Chemical2ID,
                        Chemical1Name = ar.Reaction.Chemical1.Name,
                        Chemical2Name = ar.Reaction.Chemical2.Name,
                        RelevantProduct = ar.Reaction.RelevantProduct,
                        Formula = ar.Reaction.Formula,
                        ObservationDescription = ar.Reaction.Observation.Description,
                        ImagePath = ar.Reaction.ImagePath
                    }).ToList()
            }
        },
        _ => throw new InvalidOperationException($"Unknown QuestionType {qq.Question.Type}")
    };

    private static LabChemicalDto MapLabChemical(Chemical c) => new()
    {
        ChemicalID = c.ChemicalID,
        Name = c.Name,
        Formula = c.Formula,
        ImagePath = c.ImagePath,
        Type = c.Type,
        ChemicalTypeID = (int)c.Type,
        ChemicalTypeName = c.Type == ChemicalType.Educt ? "Edukt" : "Zusatzstoff",
        Color = c.Color,
        MethodOutputs = c.MethodOutputs.ToDictionary(mo => mo.Method.Name, mo => mo.Color)
    };

    private static ChemicalDto MapChemicalToChemicalDto(Chemical c) => new()
    {
        Id = c.ChemicalID,
        Name = c.Name,
        Formula = c.Formula,
        Color = c.Color,
        MethodInfo = c.MethodOutputs.Select(mo => new MethodInfoDto
        {
            Name = mo.Method.Name,
            Color = mo.Color
        }).ToList()
    };

    public async Task<QuizPlayDto> StartNewAttempt(Guid userId, int quizId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var hasAccess = await db.Quizzes.AnyAsync(q =>
            q.QuizID == quizId &&
            (q.CreatedBy == userId || q.Groups.Any(g => g.Users.Any(u => u.UserID == userId))));
        if (!hasAccess)
            throw new UnauthorizedAccessException("User has no access to this quiz.");

        var openAttempt = await db.QuizAttempts
            .Where(a => a.UserID == userId && a.QuizID == quizId && a.Completed == default)
            .OrderByDescending(a => a.AttemptID)
            .FirstOrDefaultAsync();
        if (openAttempt is not null)
        {
            openAttempt.Completed = DateTime.Now;
        }

        var fresh = new QuizAttempt
        {
            UserID = userId,
            QuizID = quizId,
            Started = DateTime.Now
        };
        db.QuizAttempts.Add(fresh);
        await db.SaveChangesAsync();

        return await BuildQuizPlayDto(db, quizId, fresh.AttemptID);
    }

    public async Task CompleteAttempt(int attemptId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var attempt = await db.QuizAttempts.SingleAsync(a => a.AttemptID == attemptId);
        attempt.Completed = DateTime.Now;
        await db.SaveChangesAsync();
    }

}