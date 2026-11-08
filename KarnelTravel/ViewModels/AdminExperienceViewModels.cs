using System.ComponentModel.DataAnnotations;
using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public class AdminExperienceRowViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? StartingPrice { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}

public class AdminExperienceFormViewModel
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
    [Url(ErrorMessage = "Enter a valid image URL.")]
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

    public static AdminExperienceFormViewModel FromEntity(Experience experience) => new()
    {
        Id = experience.Id,
        Name = experience.Name,
        Category = experience.Category,
        Location = experience.Location,
        Duration = experience.Duration,
        ShortDescription = experience.ShortDescription,
        Description = experience.Description,
        ImageUrl = experience.ImageUrl,
        StartingPrice = experience.StartingPrice,
        Includes = experience.Includes,
        Excludes = experience.Excludes,
        IsPublished = experience.IsPublished,
        IsFeatured = experience.IsFeatured,
        DisplayOrder = experience.DisplayOrder
    };
}
