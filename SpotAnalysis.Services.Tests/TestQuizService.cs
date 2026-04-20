using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestQuizService : BaseDatabaseTest
{
    private QuizService _quizService;
    private GroupService _teacherService;

    private readonly Guid _createdBy = Guid.NewGuid();

    #region Users

    private static readonly Guid Teacher1 = Guid.Parse("9c9c2138-f945-41fa-823e-f3bd286c0fa1");

    #endregion

    [OneTimeSetUp]
    public void InitGroupService()
    {
        var logger = Substitute.For<ILogger<QuizService>>();
        _quizService = new QuizService(logger, ContextFactory);
        var dpServices = new ServiceCollection();
        dpServices.AddDataProtection();
        var dpProvider = dpServices.BuildServiceProvider()
            .GetRequiredService<IDataProtectionProvider>();
        var inviteTokens = new GroupInviteTokenService(dpProvider);
        _teacherService = new GroupService(ContextFactory, inviteTokens);
    }

    [Test]
    public async Task GetAllQuizzes_ReturnAllQuizzes()
    {
        await CleanUpDb();

        var quizCount = await CreateMultipleQuizzes();

        var quizzes = await _quizService.GetAllQuizzes();

        Assert.That(quizzes, Has.Count.EqualTo(quizCount));
    }

    [Test]
    public async Task UpdateQuiz_ReturnUpdatedName()
    {
        await CleanUpDb();

        await CreateMultipleQuizzes();

        var newQuizName = "My cool new name";

        var quizToUpdate = new UpdateQuizDto
        {
            Id = 10,
            Name = newQuizName,
            Questions = []
        };

        await _quizService.UpdateQuiz(_createdBy, quizToUpdate);

        await using var dbContext = await ContextFactory.CreateDbContextAsync();

        var quiz = await dbContext.Quizzes.SingleAsync(x => x.QuizID == 10);

        Assert.That(quiz.Name, Is.EqualTo(newQuizName));
    }

    [Test]
    public async Task DeleteQuiz_QuizNotExistingAnymore()
    {
        await CleanUpDb();

        await CreateMultipleQuizzes();

        const int deleteQuizId = 10;

        await _quizService.DeleteQuiz(_createdBy, deleteQuizId);

        await using var dbContext = await ContextFactory.CreateDbContextAsync();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = dbContext.Quizzes.Single(x => x.QuizID == 10);
        });

        Assert.That(ex.Message, Is.EqualTo("Sequence contains no elements"));
    }

    [Test]
    public async Task AssignGroupToQuiz_AssignGroup()
    {
        await CleanUpDb();

        await CreateMultipleQuizzes();

        await using var dbContext = await ContextFactory.CreateDbContextAsync();

        var result = await dbContext.Groups.AddAsync(new Group
        {
            Name = "Fetzige Group"
        });

        await dbContext.SaveChangesAsync();

        Assert.DoesNotThrowAsync(async () =>
        {
            await _quizService.AssignGroupToQuiz(_createdBy, 1, result.Entity.GroupID);
        });

        var group = await dbContext.Groups
            .Include(x => x.Quizzes)
            .SingleAsync(x => x.GroupID == result.Entity.GroupID);

        Assert.That(group.Quizzes, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task RemoveGroupFromQuiz_RemoveGroup()
    {
        await CleanUpDb();

        await CreateMultipleQuizzes();

        Group createdQuiz;

        await using (var dbContext = await ContextFactory.CreateDbContextAsync())
        {
            var quiz = await dbContext.Quizzes.FirstAsync();

            var result = await dbContext.Groups.AddAsync(new Group
            {
                Name = "Fetzige Group",
                Quizzes = [quiz]
            });

            createdQuiz = result.Entity;

            await dbContext.SaveChangesAsync();

            Assert.DoesNotThrowAsync(async () =>
            {
                await _quizService.RemoveGroupFromQuiz(_createdBy, quiz.QuizID, createdQuiz.GroupID);
            });
        }

        await using (var dbContext = await ContextFactory.CreateDbContextAsync())
        {
            var group = await dbContext.Groups
                .Include(x => x.Quizzes)
                .SingleAsync(x => x.GroupID == createdQuiz.GroupID);

            Assert.That(group.Quizzes, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public async Task GetQuestionsFromQuiz_GetQuestions()
    {
        await CleanUpDb();

        await CreateMultipleQuizzes();

        Group createdQuiz;

        await using (var dbContext = await ContextFactory.CreateDbContextAsync())
        {
            var quiz = await dbContext.Quizzes.FirstAsync();

            var result = await dbContext.Groups.AddAsync(new Group
            {
                Name = "Fetzige Group",
                Quizzes = [quiz]
            });

            createdQuiz = result.Entity;

            await dbContext.SaveChangesAsync();

            Assert.DoesNotThrowAsync(async () =>
            {
                await _quizService.RemoveGroupFromQuiz(_createdBy, quiz.QuizID, createdQuiz.GroupID);
            });
        }

        await using (var dbContext = await ContextFactory.CreateDbContextAsync())
        {
            var group = await dbContext.Groups
                .Include(x => x.Quizzes)
                .SingleAsync(x => x.GroupID == createdQuiz.GroupID);

            Assert.That(group.Quizzes, Has.Count.EqualTo(0));
        }
    }

    private async Task<int> CreateMultipleQuizzes()
    {
        await using (var dbContext = await ContextFactory.CreateDbContextAsync())
        {
            await dbContext.Users.AddAsync(new User
            {
                UserID = _createdBy,
                UserName = "Test",
                PasswordHash = "HohohoNoHash",
                Roles = [Role.Teacher]
            });

            await dbContext.SaveChangesAsync();
        }

        List<Task> tasks = [];

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_quizService.CreateQuiz(_createdBy, new CreateQuizDto
            {
                Name = i.ToString(),
                Questions = []
            }));
        }

        Task.WaitAll(tasks);

        return tasks.Count;
    }

    // private async Task<int> CreateMultipleQuestions()
    // {
    //     await using (var dbContext = await ContextFactory.CreateDbContextAsync())
    //     {
    //         await dbContext.Users.AddAsync(new User
    //         {
    //             UserID = _createdBy,
    //             UserName = "Test",
    //             PasswordHash = "HohohoNoHash"
    //         });
    //
    //         await dbContext.SaveChangesAsync();
    //     }
    //     
    //     List<Task> tasks = [];
    //     
    //     for (int i = 0; i < 100; i++)
    //     {
    //         tasks.Add(_quizService.CreateSTQuestion(_createdBy, new CreateQuizDto
    //         {
    //             Name = i.ToString(),
    //             Questions = []
    //         }));
    //     }
    //     
    //     for (int i = 0; i < 100; i++)
    //     {
    //         tasks.Add(_quizService.CreateSTLQuestion(_createdBy, new CreateQuizDto
    //         {
    //             Name = i.ToString(),
    //             Questions = []
    //         }));
    //     }
    //
    //     Task.WaitAll(tasks);
    //
    //     return tasks.Count;
    // }

    [Test]
    public async Task GetQuizzes_ReturnsCreatedQuiz()
    {
        await _teacherService.CreateGroup(Teacher1, new ConfigGroupDto
        {
            Name = "Test Quiz Group",
        });

        var groups = await _teacherService.GetGroups(Teacher1);

        Assert.That(groups, Has.Count.EqualTo(1));

        await _quizService.CreateQuiz(Teacher1, new CreateQuizDto
        {
            Name = "Test Quiz",
            Questions = [],
        });

        var quizzes = await _quizService.GetQuizzes(Teacher1);

        Assert.That(quizzes, Has.Count.EqualTo(1));

        var quiz = quizzes[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(quiz.Name, Is.EqualTo("Test Quiz"));
            Assert.That(quiz.STCount, Is.EqualTo(0));
            Assert.That(quiz.STLCount, Is.EqualTo(0));
        }

        await _quizService.CreateSTLQuestion(Teacher1, new ConfigSTLQuestionDto
        {
            Description = "A Test STL Question",
            AvailableReactions = [
                1
            ],
            ShowEductId = 1,
            ReactionId = 1,
            Title = "HohohoTitle"
        });

        await _quizService.UpdateQuiz(Teacher1, new UpdateQuizDto
        {
            Name = "Test Quiz",
            Id = quiz.Id,
            Questions = [
                new QuestionDto
                {
                    Id = 1,
                    Order = 1
                },
            ],
        });
    }

    [Test]
    public async Task GetQuizzes_NotStartedWhenNoAttempt()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);

        var result = await _quizService.GetQuizzes(creatorId);
        var q = result.Single(x => x.Id == quizId);

        Assert.That(q.LastAttemptStatus, Is.EqualTo(LastAttemptStatus.NotStarted));
        Assert.That(q.LastCompletedAt, Is.Null);
        Assert.That(q.QuestionCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetQuizzes_InProgressWhenOpenAttempt()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);
        await _quizService.StartOrResumeQuiz(creatorId, quizId);

        var q = (await _quizService.GetQuizzes(creatorId)).Single(x => x.Id == quizId);

        Assert.That(q.LastAttemptStatus, Is.EqualTo(LastAttemptStatus.InProgress));
        Assert.That(q.LastCompletedAt, Is.Null);
    }

    [Test]
    public async Task GetQuizzes_CompletedWhenAttemptCompleted()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);
        var attempt = await _quizService.StartOrResumeQuiz(creatorId, quizId);
        await _quizService.CompleteAttempt(creatorId, attempt.AttemptID);

        var q = (await _quizService.GetQuizzes(creatorId)).Single(x => x.Id == quizId);

        Assert.That(q.LastAttemptStatus, Is.EqualTo(LastAttemptStatus.Completed));
        Assert.That(q.LastCompletedAt, Is.Not.Null);
    }

    private async Task<int> SeedQuizForUser(Guid userId, Guid creatorId, bool assignViaGroup)
    {
        await using var db = await ContextFactory.CreateDbContextAsync();

        if (!await db.Users.AnyAsync(u => u.UserID == creatorId))
            db.Users.Add(new User { UserID = creatorId, UserName = "Creator", PasswordHash = "x" });

        if (userId != creatorId && !await db.Users.AnyAsync(u => u.UserID == userId))
            db.Users.Add(new User { UserID = userId, UserName = "Member", PasswordHash = "x" });

        var quiz = new SpotAnalysis.Data.Models.Quizzes.Quiz { Name = "Seed", CreatedBy = creatorId };
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        if (assignViaGroup)
        {
            var group = new SpotAnalysis.Data.Models.Identity.Group { Name = "SeedGroup" };
            group.Users.Add(await db.Users.SingleAsync(u => u.UserID == userId));
            group.Quizzes.Add(quiz);
            db.Groups.Add(group);
            await db.SaveChangesAsync();
        }

        return quiz.QuizID;
    }

    [Test]
    public async Task StartOrResumeQuiz_CreatorGetsAccess()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);

        var dto = await _quizService.StartOrResumeQuiz(creatorId, quizId);

        Assert.That(dto.QuizID, Is.EqualTo(quizId));
        Assert.That(dto.AttemptID, Is.GreaterThan(0));
    }

    [Test]
    public async Task StartOrResumeQuiz_GroupMemberGetsAccess()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(memberId, creatorId, assignViaGroup: true);

        var dto = await _quizService.StartOrResumeQuiz(memberId, quizId);

        Assert.That(dto.QuizID, Is.EqualTo(quizId));
    }

    [Test]
    public async Task StartOrResumeQuiz_StrangerIsRejected()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);

        await using (var db = await ContextFactory.CreateDbContextAsync())
        {
            db.Users.Add(new User { UserID = strangerId, UserName = "Stranger", PasswordHash = "x" });
            await db.SaveChangesAsync();
        }

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _quizService.StartOrResumeQuiz(strangerId, quizId));
    }

    [Test]
    public async Task StartNewAttempt_ClosesOpenAttemptAndCreatesNew()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);

        var first = await _quizService.StartOrResumeQuiz(creatorId, quizId);
        var second = await _quizService.StartNewAttempt(creatorId, quizId);

        Assert.That(second.AttemptID, Is.Not.EqualTo(first.AttemptID));

        await using var db = await ContextFactory.CreateDbContextAsync();
        var closed = await db.QuizAttempts.SingleAsync(a => a.AttemptID == first.AttemptID);
        Assert.That(closed.Completed, Is.GreaterThan(DateTime.Now.AddMinutes(-1)));
    }

    [Test]
    public async Task StartNewAttempt_WithoutOpenAttempt_JustCreates()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);

        var dto = await _quizService.StartNewAttempt(creatorId, quizId);

        Assert.That(dto.AttemptID, Is.GreaterThan(0));
    }

    [Test]
    public async Task StartOrResumeQuiz_ResumesOpenAttempt()
    {
        await CleanUpDb();
        var creatorId = Guid.NewGuid();
        var quizId = await SeedQuizForUser(creatorId, creatorId, assignViaGroup: false);

        var first = await _quizService.StartOrResumeQuiz(creatorId, quizId);
        var second = await _quizService.StartOrResumeQuiz(creatorId, quizId);

        Assert.That(second.AttemptID, Is.EqualTo(first.AttemptID));
    }

    [Test]
    public async Task CompleteAttempt_SetsCompletedNow()
    {
        await CleanUpDb();

        int attemptId;
        await using (var db = await ContextFactory.CreateDbContextAsync())
        {
            await db.Users.AddAsync(new User { UserID = _createdBy, UserName = "U", PasswordHash = "x" });
            var quiz = new SpotAnalysis.Data.Models.Quizzes.Quiz { Name = "Q", CreatedBy = _createdBy };
            db.Quizzes.Add(quiz);
            await db.SaveChangesAsync();

            var attempt = new SpotAnalysis.Data.Models.Quizzes.QuizAttempt
            {
                UserID = _createdBy,
                QuizID = quiz.QuizID,
                Started = DateTime.Now
            };
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
            attemptId = attempt.AttemptID;
        }

        await _quizService.CompleteAttempt(_createdBy, attemptId);

        await using (var db = await ContextFactory.CreateDbContextAsync())
        {
            var updated = await db.QuizAttempts.SingleAsync(a => a.AttemptID == attemptId);
            Assert.That(updated.Completed, Is.GreaterThan(DateTime.Now.AddMinutes(-1)));
        }
    }
}