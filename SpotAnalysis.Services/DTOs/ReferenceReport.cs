namespace SpotAnalysis.Services.DTOs;

public class ReferenceReport
{
    public int Total => Items.Count;
    public List<ReferenceItem> Items { get; set; } = new();
}

public class ReferenceItem
{
    public required string Kind { get; init; }
    public required int Id { get; init; }
    public required string Description { get; init; }
    public string? RouteTemplate { get; init; }
}
