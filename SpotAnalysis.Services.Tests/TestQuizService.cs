using Microsoft.Extensions.Logging;
using NSubstitute;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestQuizService : BaseDatabaseTest
{
    private QuizService _quizService;
    
    private readonly Guid CreatedBy = Guid.NewGuid();
    
    [OneTimeSetUp]
    public void InitTeacherService()
    {
        var logger = Substitute.For<ILogger<QuizService>>();
        _quizService = new QuizService(logger, ContextFactory);
    }
    
    [Test]
    public async Task GetAllQuizzes_ReturnAllQuizzes()
    {
        var quizCount = await CreateMultipleQuizzes();

        var quizzes = await _quizService.GetAllQuizzes();
        
        Assert.That(quizzes, Has.Count.EqualTo(quizCount));
    }

    private async Task<int> CreateMultipleQuizzes()
    {
        await using (var dbContext = await ContextFactory.CreateDbContextAsync())
        {
            await dbContext.Users.AddAsync(new User
            {
                UserID = CreatedBy,
                UserName = "Test",
                PasswordHash = "HohohoNoHash"
            });

            await dbContext.SaveChangesAsync();
        }
        
        List<Task> tasks = [];
        
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_quizService.CreateQuiz(CreatedBy, new CreateQuizDto
            {
                Name = i.ToString(),
                Questions = [],
                AssignedGroupsIds = []
            }));
        }

        Task.WaitAll(tasks);

        return tasks.Count;
    }
}