using Serilog;
using SpotAnalysis.Services;
using SpotAnalysis.Services.Services;
using SpotAnalysis.Web.Components;
using SpotAnalysis.Web.Extensions;

namespace SpotAnalysis.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSpotAnalysis(builder.Configuration);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddWebAuthentication();

        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        });

        var app = builder.Build();

        app.UseSerilogRequestLogging();

        using (var scope = app.Services.CreateAsyncScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            try
            {
                await seeder.SeedAdminAsync();
                await seeder.SeedMasterDataAsync();

                if (app.Environment.IsDevelopment())
                {
                    await seeder.SeedDevUserAsync();
                    await seeder.SeedQuizDataAsync();
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Seeding failed");
            }
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapAuthEndpoints();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}