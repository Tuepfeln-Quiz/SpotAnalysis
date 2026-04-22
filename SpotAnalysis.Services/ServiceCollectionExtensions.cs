using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            {
                options.UseNpgsql(configuration.GetConnectionString("MyDatabase"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            }
        );

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupInviteTokenService, GroupInviteTokenService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IUsernameService, UsernameService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IXlsImportExportService, XlsImportExportService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IChemistryDataService, ChemistryDataService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

        return services;
    }
}
