using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class Experience
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Category { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Duration { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ShortDescription { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, 100000000)]
    public decimal? StartingPrice { get; set; }

    [StringLength(1000)]
    public string? Includes { get; set; }

    [StringLength(1000)]
    public string? Excludes { get; set; }

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
