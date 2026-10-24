
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class TransportService
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string VehicleType { get; set; } = string.Empty;

    [StringLength(150)]
    public string StartingCity { get; set; } = string.Empty;

    [StringLength(150)]
    public string DestinationCity { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int? PassengerCapacity { get; set; }

    [Range(0, 100000000)]
    public decimal? StartingPrice { get; set; }

    [StringLength(100)]
    public string ContactPhone { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
