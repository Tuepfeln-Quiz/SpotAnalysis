using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Quizzes;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class QuizService(IDbContextFactory<AnalysisContext> factory) : IQuizService
{
    public List<QuizDto> GetQuizzes()
    {
        throw new NotImplementedException();
    }

    public void CreateQuiz(ConfigQuizDto quiz)
    {
        throw new NotImplementedException();
    }

    public void UpdateQuiz(ConfigQuizDto quiz)
    {
        throw new NotImplementedException();
    }

    public void DeleteQuiz(int quizId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<QuizOverviewDto>> GetQuizzes(Guid studentId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return context.Users.Where(u => u.UserID == studentId)
            .SelectMany(u => u.Groups)
            .SelectMany(g => g.Quizzes)
            .Select(q => new QuizOverviewDto
            {
                Id = q.QuizID,
                Name = q.Name,
                STCount = q.Questions.Select(qq => qq.Type == QuestionType.SpotTest).Count(),
                STLCount = q.Questions.Select(qq => qq.Type == QuestionType.SpotTestLight).Count(),
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
        
        return await context.Quizzes.Where(q => q.QuizID == quizId)
            .Select(q => new QuizDto
            {
                Name = q.Name,
                STLQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTestLight).Select(STLQuestionDto.FromQuestion).ToList(),
                STQuestions = q.QuizQuestions.Where(qq => qq.Question.Type == QuestionType.SpotTest).Select(STQuestionDto.FromQuestion).ToList(),
            }).SingleAsync();
    }

    public async Task ValidateAndSaveQuestion(ValAndSaveQuestionDto questionResult)
    {
        throw new NotImplementedException();
    }
}