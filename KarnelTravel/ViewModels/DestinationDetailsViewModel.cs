
using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public class DestinationDetailsViewModel
{
    public Destination Destination { get; set; } = new();

    public List<Hotel> Hotels { get; set; } = new();

    public List<Restaurant> Restaurants { get; set; } = new();

    public List<Resort> Resorts { get; set; } = new();

    public List<TourPackage> Packages { get; set; } = new();

    public List<Review> Reviews { get; set; } = new();

    public double AverageRating { get; set; }

    public int TotalReviews { get; set; }
}
