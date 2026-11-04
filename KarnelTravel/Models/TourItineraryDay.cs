
using System;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class TourItineraryDay
{
    public int Id { get; set; }

    [Required]
    public int TourPackageId { get; set; }

    [Range(1, 365)]
    public int DayNumber { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(150)]
    public string? OvernightLocation { get; set; }

    public int DisplayOrder { get; set; }

    public TourPackage? TourPackage { get; set; }
}
