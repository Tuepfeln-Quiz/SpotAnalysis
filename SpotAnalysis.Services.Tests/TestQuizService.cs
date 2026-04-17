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
            await _quizService.AssignGroupToQuiz(1, result.Entity.GroupID);
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
                await _quizService.RemoveGroupFromQuiz(quiz.QuizID, createdQuiz.GroupID);
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
                await _quizService.RemoveGroupFromQuiz(quiz.QuizID, createdQuiz.GroupID);
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
    public async Task OpenQuiz()
    {
        {
            await _teacherService.CreateGroup(Teacher1, new ConfigGroupDto
            {
                Name = "Test Quiz Group",
            });

            var groups = await _teacherService.GetGroups(Teacher1);
        
            Assert.That(groups, Has.Count.EqualTo(1));
        
            var gid = groups.First().Id;

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

            var opened = await _quizService.OpenQuiz(Teacher1, quiz.Id);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(opened.Name, Is.EqualTo("Test Quiz"));
                Assert.That(opened.STQuestions, Has.Count.EqualTo(0));
                Assert.That(opened.STLQuestions, Has.Count.EqualTo(0));
            }
        
            var attempt = await _quizService.GetQuizAttempt(Teacher1, quiz.Id);
        
            Assert.That(attempt, Is.Not.Null);
        
            Assert.That(attempt.Completed, Is.EqualTo(DateTime.Parse("0001-01-01 00:00:00")));
        }

        {
            var quizzes = await _quizService.GetQuizzes(Teacher1);
        
            Assert.That(quizzes, Has.Count.EqualTo(1));
        
            var quiz = quizzes[0];

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
    }
    
    private async Task CleanUpDb()
    {
        await using var dbContext = await ContextFactory.CreateDbContextAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        await SeedDatabase();
    }
}