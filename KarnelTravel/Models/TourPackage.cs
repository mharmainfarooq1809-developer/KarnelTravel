using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class TourPackage
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string DestinationName { get; set; } = string.Empty;

    public decimal? PricePerAdult { get; set; }
    public decimal? PricePerChild { get; set; }

    [StringLength(300)]
    public string? ImagePath { get; set; }

    [StringLength(1000)]
    public string? ShortDescription { get; set; }

    [StringLength(500)]
    public string? IncludedServices { get; set; }

    public int DurationDays { get; set; }

    [StringLength(80)]
    public string? PriceBasis { get; set; }

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}