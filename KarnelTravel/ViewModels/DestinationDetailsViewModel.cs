
using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public class DestinationDetailsViewModel
{
    public Destination Destination { get; set; } = new();

    public List<Hotel> Hotels { get; set; } = new();

    public List<Restaurant> Restaurants { get; set; } = new();

    public List<Resort> Resorts { get; set; } = new();
}
