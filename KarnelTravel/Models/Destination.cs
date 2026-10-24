using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class Destination
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ImagePath { get; set; }

    [StringLength(1000)]
    public string? ShortDescription { get; set; }

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}