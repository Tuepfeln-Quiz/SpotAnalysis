using NUnit.Framework;
using SpotAnalysis.Data;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class QuizServiceQuestionTests : BaseDatabaseTest
{
    private QuizService _quizService = null!;

    [OneTimeSetUp]
    public void ServiceSetUp()
    {
        _quizService = new QuizService(
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<QuizService>(),
            ContextFactory);
    }

    [Test]
    public async Task GetQuestions_ReturnsAllQuestions()
    {
        var result = await _quizService.GetQuestions();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<DTOs.QuestionOverviewDto>>());
    }
}
