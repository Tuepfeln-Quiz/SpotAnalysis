using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotAnalysis.Data;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registriert DbContext und alle SpotAnalysis-Services.
    /// Web muss dadurch SpotAnalysis.Data nicht direkt referenzieren.
    /// </summary>
    public static IServiceCollection AddSpotAnalysis(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<AnalysisContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MyDatabase")));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IUsernameService, UsernameService>();
        services.AddScoped<IXlsImportExportService, XlsImportExportService>();
        services.AddScoped<IChemistryDataService, ChemistryDataService>();

        return services;
    }
}
