using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using SpotAnalysis.Services;
using SpotAnalysis.Services.Services;
using SpotAnalysisV2.Web.Components;
using SpotAnalysisV2.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpotAnalysis(builder.Configuration);

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

await using (var scope = app.Services.CreateAsyncScope())
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
    catch (InvalidOperationException ex)
    {
        app.Logger.LogError(ex, "Seeding failed");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownNetworks = { },
        KnownProxies = { },
    });
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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