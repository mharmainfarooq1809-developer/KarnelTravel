
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KarnelTravel.ViewModels;

public class AdminPropertyFormViewModel
{
    [Required(ErrorMessage = "Select a property type.")]
    public string Category { get; set; } = "Hotel";

    [Required(ErrorMessage = "Property name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "City or region is required.")]
    [StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Short description is required.")]
    [StringLength(1000)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full description is required.")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Property image")]
    public IFormFile? ImageFile { get; set; }

    [Range(0, 100000000, ErrorMessage = "Enter a valid price.")]
    public decimal? Price { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int? StarRating { get; set; }

    [StringLength(100)]
    public string? CuisineType { get; set; }

    [Required(ErrorMessage = "Contact phone is required.")]
    [StringLength(100)]
    public string ContactPhone { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }

    [Range(0, 100000)]
    public int DisplayOrder { get; set; }

    [Required(ErrorMessage = "Destination ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Enter a valid positive destination ID.")]
    public int DestinationId { get; set; }
}
