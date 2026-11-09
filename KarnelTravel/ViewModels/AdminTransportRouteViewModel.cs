using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public class AdminTransportRouteViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string StartingCity { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string DestinationCity { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal StartingPricePKR { get; set; }

    [Range(0.01, 100000000)]
    public decimal? ReturnPricePKR { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 100000)]
    public decimal? EstimatedDistanceKm { get; set; }

    [Range(0, 100000)]
    public int? EstimatedDurationMinutes { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public int[] VehicleIds { get; set; } = Array.Empty<int>();
}
