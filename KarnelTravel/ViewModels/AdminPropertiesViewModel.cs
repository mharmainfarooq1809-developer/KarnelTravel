
using System.Collections.Generic;

namespace KarnelTravel.ViewModels;

public class AdminPropertiesViewModel
{
    public List<AdminPropertyRowViewModel> Properties { get; set; } = new();

    public int HotelCount { get; set; }

    public int RestaurantCount { get; set; }

    public int ResortCount { get; set; }

    public int PublishedCount { get; set; }

    public int DraftCount { get; set; }
}

public class AdminPropertyRowViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal? Price { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }

    public string? DestinationName { get; set; }
}
