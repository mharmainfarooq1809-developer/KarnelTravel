
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class Resort
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string CityOrRegion { get; set; } = string.Empty;

    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    public string ShortDescription { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal? StartingPricePerNight { get; set; }

    [StringLength(100)]
    public string ContactPhone { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
