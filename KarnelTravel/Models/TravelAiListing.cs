namespace KarnelTravel.Models;

public sealed class TravelAiListing
{
    public string Type { get; init; } = string.Empty;
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public bool IsFeatured { get; init; }
    public string PriceLabel { get; init; } = "Cost unavailable";
}
