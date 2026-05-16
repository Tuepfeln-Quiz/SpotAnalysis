using Microsoft.Extensions.Logging;
using NSubstitute;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestSessionService : BaseDatabaseTest
{
    private static readonly Guid TeacherId = Guid.Parse("9c9c2138-f945-41fa-823e-f3bd286c0fa1");
    private static readonly Guid StudentId = Guid.Parse("2195c82c-0a67-4938-9c88-20c089276da5");
    private ISessionService _sessionService;

    [OneTimeSetUp]
    public void InitSessionService()
    {
        var logger = Substitute.For<ILogger<SessionService>>();
        _sessionService = new SessionService(logger, ContextFactory);
    }

    [TearDown]
    public async Task CleanSessions() => await CleanUpDb();

    [Test]
    public async Task CreateSession_ReturnsSessionWithCorrectProperties()
    {
        var duration = TimeSpan.FromHours(1);

        var session = await _sessionService.CreateSession(TeacherId, duration, "TestAgent", "127.0.0.1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(session.SessionId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(session.UserId, Is.EqualTo(TeacherId));
            Assert.That(session.UserAgent, Is.EqualTo("TestAgent"));
            Assert.That(session.IpAddress, Is.EqualTo("127.0.0.1"));
            Assert.That(session.Expiry, Is.GreaterThan(DateTime.UtcNow));
        }
    }

    [Test]
    public async Task GetSession_ReturnsCreatedSession()
    {
        var created = await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));

        var fetched = await _sessionService.GetSession(created.SessionId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fetched.SessionId, Is.EqualTo(created.SessionId));
            Assert.That(fetched.UserId, Is.EqualTo(TeacherId));
        }
    }

    [Test]
    public async Task GetUserSessions_ReturnsOnlyUsersSessions()
    {
        await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));
        await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(2));
        await _sessionService.CreateSession(StudentId, TimeSpan.FromHours(1));

        var teacherSessions = await _sessionService.GetUserSessions(TeacherId);
        var studentSessions = await _sessionService.GetUserSessions(StudentId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teacherSessions, Has.Count.EqualTo(2));
            Assert.That(studentSessions, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task GetAllSessions_ReturnsAll()
    {
        await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));
        await _sessionService.CreateSession(StudentId, TimeSpan.FromHours(1));

        var all = await _sessionService.GetAllSessions();

        Assert.That(all, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ValidateSession_ValidSession_ReturnsTrue()
    {
        var session = await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));

        var result = await _sessionService.ValidateSession(
            session.SessionId, TeacherId, new HashSet<string> { nameof(Role.Teacher) });

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ValidateSession_ExpiredSession_ReturnsFalse()
    {
        var session = await _sessionService.CreateSession(TeacherId, TimeSpan.FromMilliseconds(-1));

        var result = await _sessionService.ValidateSession(
            session.SessionId, TeacherId, new HashSet<string> { nameof(Role.Teacher) });

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateSession_WrongUser_ReturnsFalse()
    {
        var session = await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));

        var result = await _sessionService.ValidateSession(
            session.SessionId, StudentId, new HashSet<string> { nameof(Role.Teacher) });

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateSession_WrongRole_ReturnsFalse()
    {
        var session = await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));

        var result = await _sessionService.ValidateSession(
            session.SessionId, TeacherId, new HashSet<string> { nameof(Role.Admin) });

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateSession_NonexistentSession_ReturnsFalse()
    {
        var result = await _sessionService.ValidateSession(
            Guid.NewGuid(), TeacherId, new HashSet<string> { nameof(Role.Teacher) });

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task InvalidateSession_RemovesSpecificSession()
    {
        var session1 = await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));
        var session2 = await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));

        await _sessionService.InvalidateSession(session1.SessionId);

        var remaining = await _sessionService.GetUserSessions(TeacherId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remaining, Has.Count.EqualTo(1));
            Assert.That(remaining[0].SessionId, Is.EqualTo(session2.SessionId));
        }
    }

    [Test]
    public async Task InvalidateAllForUser_RemovesAllUserSessions()
    {
        await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(1));
        await _sessionService.CreateSession(TeacherId, TimeSpan.FromHours(2));
        await _sessionService.CreateSession(StudentId, TimeSpan.FromHours(1));

        await _sessionService.InvalidateAllForUser(TeacherId);

        var teacherSessions = await _sessionService.GetUserSessions(TeacherId);
        var studentSessions = await _sessionService.GetUserSessions(StudentId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teacherSessions, Has.Count.EqualTo(0));
            Assert.That(studentSessions, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task CleanupSessions_RemovesOnlyExpiredSessions()
    {
        await _sessionService.CreateSession(TeacherId, TimeSpan.FromMilliseconds(-1));
        var validSession = await _sessionService.CreateSession(StudentId, TimeSpan.FromHours(1));

        await _sessionService.CleanupSessions();

        var all = await _sessionService.GetAllSessions();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(all, Has.Count.EqualTo(1));
            Assert.That(all[0].SessionId, Is.EqualTo(validSession.SessionId));
        }
    }
}
