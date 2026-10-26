
namespace KarnelTravel.Models;

public class TourPackage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? DestinationName { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DurationDays { get; set; }
    public decimal? PricePerAdult { get; set; }
    public decimal? PricePerChild { get; set; }
    public string? Inclusions { get; set; }
    public string? Exclusions { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? DestinationId { get; set; }
    public Destination? Destination { get; set; }
}
