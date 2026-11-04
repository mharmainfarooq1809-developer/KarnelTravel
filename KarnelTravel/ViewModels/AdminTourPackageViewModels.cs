using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KarnelTravel.ViewModels;

public class AdminTourPackageRowViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? DestinationName { get; set; }

    public string? ImageUrl { get; set; }

    public int DurationDays { get; set; }

    public decimal? PricePerAdult { get; set; }

    public decimal? PricePerChild { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }

    public int DepartureCount { get; set; }

    public int DisplayOrder { get; set; }
}

public class AdminTourPackageFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Package name is required.")]
    [StringLength(150)]
    [Display(Name = "Package name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL slug is required.")]
    [StringLength(150)]
    [RegularExpression(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Use lowercase letters, numbers and hyphens only.")]
    [Display(Name = "URL slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Destination")]
    public int? DestinationId { get; set; }

    [Required(ErrorMessage = "Short description is required.")]
    [StringLength(1000)]
    [Display(Name = "Short description")]
    public string? ShortDescription { get; set; }

    [StringLength(3000)]
    [Display(Name = "Full description")]
    public string? Description { get; set; }

    [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days.")]
    [Display(Name = "Duration in days")]
    public int DurationDays { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Adult price (PKR)")]
    public decimal? PricePerAdult { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Child price (PKR)")]
    public decimal? PricePerChild { get; set; }

    [StringLength(2000)]
    public string? Inclusions { get; set; }

    [StringLength(2000)]
    public string? Exclusions { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Package image")]
    public IFormFile? ImageFile { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }
}