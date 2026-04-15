using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class QuizServiceQuestionTests : BaseDatabaseTest
{
    private QuizService _quizService = null!;

    // Lehrer1 from seed.sql — valid FK for CreatedBy
    private static readonly Guid SeededTeacherId = Guid.Parse("9c9c2138-f945-41fa-823e-f3bd286c0fa1");

    private int _chemical1Id;
    private int _chemical2Id;
    private int _methodId;
    private int _reaction1Id;
    private int _reaction2Id;
    private int _reaction3Id;

    [OneTimeSetUp]
    public async Task ServiceSetUp()
    {
        _quizService = new QuizService(
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<QuizService>(),
            ContextFactory);

        await using var dbContext = await ContextFactory.CreateDbContextAsync();

        // Seed chemicals
        var chem1 = new Chemical { Name = "Chem1", Formula = "C1", Color = "rot", Type = Data.Enums.ChemicalType.Educt };
        var chem2 = new Chemical { Name = "Chem2", Formula = "C2", Color = "blau", Type = Data.Enums.ChemicalType.Educt };
        await dbContext.Chemicals.AddRangeAsync(chem1, chem2);
        await dbContext.SaveChangesAsync();
        _chemical1Id = chem1.ChemicalID;
        _chemical2Id = chem2.ChemicalID;

        // Seed method
        var method = new Method { Name = "TestMethod" };
        await dbContext.Methods.AddAsync(method);
        await dbContext.SaveChangesAsync();
        _methodId = method.MethodID;

        // Seed observations and reactions (Reaction requires Chemical1, Chemical2, Observation)
        var obs1 = new Observation { Description = "Obs1" };
        var obs2 = new Observation { Description = "Obs2" };
        var obs3 = new Observation { Description = "Obs3" };
        await dbContext.Observations.AddRangeAsync(obs1, obs2, obs3);
        await dbContext.SaveChangesAsync();

        var reaction1 = new Reaction { Chemical1ID = chem1.ChemicalID, Chemical2ID = chem2.ChemicalID, ObservationID = obs1.ObservationID, RelevantProduct = "P1", Formula = "F1" };
        var reaction2 = new Reaction { Chemical1ID = chem1.ChemicalID, Chemical2ID = chem2.ChemicalID, ObservationID = obs2.ObservationID, RelevantProduct = "P2", Formula = "F2" };
        var reaction3 = new Reaction { Chemical1ID = chem1.ChemicalID, Chemical2ID = chem2.ChemicalID, ObservationID = obs3.ObservationID, RelevantProduct = "P3", Formula = "F3" };
        await dbContext.Reactions.AddRangeAsync(reaction1, reaction2, reaction3);
        await dbContext.SaveChangesAsync();
        _reaction1Id = reaction1.ReactionID;
        _reaction2Id = reaction2.ReactionID;
        _reaction3Id = reaction3.ReactionID;
    }

    [Test]
    public async Task GetQuestions_ReturnsAllQuestions()
    {
        var result = await _quizService.GetQuestions();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<DTOs.QuestionOverviewDto>>());
    }

    [Test]
    public async Task CreateSTQuestion_CreatesQuestionWithChemicalsAndMethods()
    {
        var dto = new DTOs.ConfigSTQuestionDto
        {
            Description = "Test ST Question",
            AvailableChemicals = new List<int> { _chemical1Id, _chemical2Id },
            AvailableMethods = new List<int> { _methodId }
        };

        await _quizService.CreateSTQuestion(SeededTeacherId, dto);

        var questions = await _quizService.GetQuestions();
        var created = questions.FirstOrDefault(q => q.Description == "Test ST Question");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Type, Is.EqualTo(Data.Enums.QuestionType.SpotTest));
        Assert.That(created.ChemicalCount, Is.EqualTo(2));
        Assert.That(created.MethodCount, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateSTLQuestion_CreatesQuestionWithReactions()
    {
        var dto = new DTOs.ConfigSTLQuestionDto
        {
            Description = "Test STL Question",
            ReactionId = _reaction1Id,
            AvailableReactions = new List<int> { _reaction1Id, _reaction2Id, _reaction3Id }
        };

        await _quizService.CreateSTLQuestion(SeededTeacherId, dto);

        var questions = await _quizService.GetQuestions();
        var created = questions.FirstOrDefault(q => q.Description == "Test STL Question");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Type, Is.EqualTo(Data.Enums.QuestionType.SpotTestLight));
        Assert.That(created.ReactionCount, Is.EqualTo(3));
    }

    [Test]
    public async Task DeleteQuestion_WhenUsedInQuiz_ThrowsInvalidOperationException()
    {
        // Create a question
        await _quizService.CreateSTQuestion(SeededTeacherId, new DTOs.ConfigSTQuestionDto
        {
            Description = "Question to protect",
            AvailableChemicals = new List<int> { _chemical1Id },
            AvailableMethods = new List<int> { _methodId }
        });

        var questions = await _quizService.GetQuestions();
        var question = questions.First(q => q.Description == "Question to protect");

        // Create a quiz that uses this question
        await _quizService.CreateQuiz(SeededTeacherId, new DTOs.CreateQuizDto
        {
            Name = "Quiz with protected question",
            Questions = new List<DTOs.QuestionDto>
            {
                new() { Id = question.Id, Order = 0 }
            },
            AssignedGroupsIds = new List<int>()
        });

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _quizService.DeleteQuestion(question.Id));
    }

    [Test]
    public async Task DeleteQuestion_WhenNotUsed_DeletesSuccessfully()
    {
        await _quizService.CreateSTQuestion(SeededTeacherId, new DTOs.ConfigSTQuestionDto
        {
            Description = "Question to delete",
            AvailableChemicals = new List<int> { _chemical1Id },
            AvailableMethods = new List<int> { _methodId }
        });

        var questions = await _quizService.GetQuestions();
        var question = questions.First(q => q.Description == "Question to delete");

        await _quizService.DeleteQuestion(question.Id);

        var afterDelete = await _quizService.GetQuestions();
        Assert.That(afterDelete.Any(q => q.Id == question.Id), Is.False);
    }
}
