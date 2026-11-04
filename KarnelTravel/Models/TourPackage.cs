
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class TourPackage
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(150)]
    public string? DestinationName { get; set; }

    [StringLength(1000)]
    public string? ShortDescription { get; set; }

    [StringLength(3000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Range(1, 365)]
    public int DurationDays { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999")]
    public decimal? PricePerAdult { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal? PricePerChild { get; set; }

    [StringLength(2000)]
    public string? Inclusions { get; set; }

    [StringLength(2000)]
    public string? Exclusions { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int? DestinationId { get; set; }

    public Destination? Destination { get; set; }

    public ICollection<TourItineraryDay> ItineraryDays { get; set; }
        = new List<TourItineraryDay>();

    public ICollection<TourDeparture> Departures { get; set; }
        = new List<TourDeparture>();
}
