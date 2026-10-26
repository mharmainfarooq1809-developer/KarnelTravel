
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? keyword,
        string? category,
        decimal? budget)
    {
        keyword = keyword?.Trim();

        if (string.IsNullOrWhiteSpace(keyword))
            keyword = null;

        category = category?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(category))
            category = "all";

        ViewBag.Keyword = keyword;
        ViewBag.Category = category;
        ViewBag.Budget = budget;

        bool all = category == "all";

        var destinations = new List<Destination>();
        var hotels = new List<Hotel>();
        var restaurants = new List<Restaurant>();
        var resorts = new List<Resort>();
        var transport = new List<TransportService>();
        var packages = new List<TourPackage>();

        if (all || category == "destinations")
        {
            var query = _context.Destinations
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.Region != null && x.Region.Contains(keyword)));
            }

            destinations = await query
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        if (all || category == "hotels")
        {
            var query = _context.Hotels
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.City != null && x.City.Contains(keyword)));
            }

            if (budget.HasValue)
            {
                query = query.Where(x =>
                    x.StartingPricePerNight.HasValue &&
                    x.StartingPricePerNight.Value <= budget.Value);
            }

            hotels = await query.OrderBy(x => x.DisplayOrder).ToListAsync();
        }

        if (all || category == "restaurants")
        {
            var query = _context.Restaurants
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.City != null && x.City.Contains(keyword)) ||
                    (x.CuisineType != null &&
                     x.CuisineType.Contains(keyword)));
            }

            if (budget.HasValue)
            {
                query = query.Where(x =>
                    x.AverageMealPrice.HasValue &&
                    x.AverageMealPrice.Value <= budget.Value);
            }

            restaurants = await query
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        if (all || category == "resorts")
        {
            var query = _context.Resorts
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.CityOrRegion != null &&
                     x.CityOrRegion.Contains(keyword)));
            }

            if (budget.HasValue)
            {
                query = query.Where(x =>
                    x.StartingPricePerNight.HasValue &&
                    x.StartingPricePerNight.Value <= budget.Value);
            }

            resorts = await query
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        if (all || category == "transport")
        {
            var query = _context.TransportServices
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.VehicleType != null &&
                     x.VehicleType.Contains(keyword)) ||
                    (x.StartingCity != null &&
                     x.StartingCity.Contains(keyword)) ||
                    (x.DestinationCity != null &&
                     x.DestinationCity.Contains(keyword)));
            }

            if (budget.HasValue)
            {
                query = query.Where(x =>
                    x.StartingPrice.HasValue &&
                    x.StartingPrice.Value <= budget.Value);
            }

            transport = await query
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        if (all || category == "packages")
        {
            var query = _context.TourPackages
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.DestinationName != null &&
                     x.DestinationName.Contains(keyword)));
            }

            if (budget.HasValue)
            {
                query = query.Where(x =>
                    x.PricePerAdult.HasValue &&
                    x.PricePerAdult.Value <= budget.Value);
            }

            packages = await query
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        ViewBag.Destinations = destinations;
        ViewBag.Hotels = hotels;
        ViewBag.Restaurants = restaurants;
        ViewBag.Resorts = resorts;
        ViewBag.Transport = transport;
        ViewBag.Packages = packages;

        ViewBag.TotalResults =
            destinations.Count +
            hotels.Count +
            restaurants.Count +
            resorts.Count +
            transport.Count +
            packages.Count;

        return View();
    }

    public IActionResult Advance()
    {
        return View();
    }

    public IActionResult Advanced()
    {
        return View();
    }
}
