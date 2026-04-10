using Microsoft.Extensions.Configuration;

namespace SpotAnalysis.Services.Tests;

internal static class TestConfiguration
{
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("testsettings.json")
        .Build();

    public static string GetConnectionString(string name) =>
        Config.GetConnectionString(name)
        ?? throw new InvalidOperationException($"Connection string '{name}' not found in testsettings.json");
}
