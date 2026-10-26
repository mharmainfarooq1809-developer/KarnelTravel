
namespace KarnelTravel.Models;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? StartingPricePerNight { get; set; }
    public int? StarRating { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? DestinationId { get; set; }
    public Destination? Destination { get; set; }
}
