
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;

    private const int PageSize = 6;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet("/Search/Advanced")]
    public IActionResult Advanced() => Redirect($"{Url.Action(nameof(Index))}#discovery");

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(
        string? keyword,
        string? category = "all",
        string? location = null,
        decimal? minBudget = null,
        decimal? budget = null,
        string? sort = "recommended",
        int page = 1)
    {
        keyword = Clean(keyword);
        location = Clean(location);

        category = Clean(category)?.ToLowerInvariant() ?? "all";
        sort = Clean(sort)?.ToLowerInvariant() ?? "recommended";

        if (budget < 0) budget = null;
        if (minBudget < 0) minBudget = null;

        if (minBudget.HasValue && budget.HasValue &&
            minBudget > budget)
        {
            minBudget = budget;
        }

        page = Math.Max(1, page);

        var validCategories = new[]
        {
            "all", "destinations", "hotels",
            "restaurants", "resorts", "transport", "packages"
        };

        if (!validCategories.Contains(category))
            category = "all";

        if (!new[] { "recommended", "price-low", "price-high" }
            .Contains(sort))
        {
            sort = "recommended";
        }

        ViewBag.Keyword = keyword;
        ViewBag.Category = category;
        ViewBag.Location = location;
        ViewBag.Budget = budget;
        ViewBag.MinBudget = minBudget;
        ViewBag.Sort = sort;
        ViewBag.Page = page;
        ViewBag.PageSize = PageSize;

        bool all = category == "all";

        var destinations = new List<Destination>();
        var hotels = new List<Hotel>();
        var restaurants = new List<Restaurant>();
        var resorts = new List<Resort>();
        var transport = new List<TransportRoute>();
        var packages = new List<TourPackage>();

        int destinationCount = 0;
        int hotelCount = 0;
        int restaurantCount = 0;
        int resortCount = 0;
        int transportCount = 0;
        int packageCount = 0;

        // DESTINATIONS
        if (all || category == "destinations")
        {
            var query = _context.Destinations
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.Region != null && x.Region.Contains(keyword)) ||
                    (x.ShortDescription != null &&
                     x.ShortDescription.Contains(keyword)));
            }

            if (location != null)
            {
                query = query.Where(x =>
                    x.Region != null &&
                    x.Region.Contains(location));
            }

            destinationCount = await query.CountAsync();

            destinations = await ApplyOrdering(query, sort, x => 0)
                .ThenBy(x => x.DisplayOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // HOTELS
        if (all || category == "hotels")
        {
            var query = _context.Hotels
                .AsNoTracking()
                .Where(x => x.IsPublished && !x.IsArchived);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.City != null && x.City.Contains(keyword)) ||
                    (x.Address != null && x.Address.Contains(keyword)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(keyword)));
            }

            if (location != null)
            {
                query = query.Where(x =>
                    (x.City != null && x.City.Contains(location)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(location)) ||
                    (x.Destination != null &&
                     x.Destination.Region != null &&
                     x.Destination.Region.Contains(location)));
            }

            if (minBudget.HasValue)
                query = query.Where(x =>
                    x.StartingPricePerNight >= minBudget.Value);

            if (budget.HasValue)
                query = query.Where(x =>
                    x.StartingPricePerNight <= budget.Value);

            hotelCount = await query.CountAsync();

            hotels = await ApplyOrdering(
                    query, sort, x => x.StartingPricePerNight ?? 0)
                .ThenBy(x => x.DisplayOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // RESTAURANTS
        if (all || category == "restaurants")
        {
            var query = _context.Restaurants
                .AsNoTracking()
                .Where(x => x.IsPublished && !x.IsArchived);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.City != null && x.City.Contains(keyword)) ||
                    (x.CuisineType != null &&
                     x.CuisineType.Contains(keyword)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(keyword)));
            }

            if (location != null)
            {
                query = query.Where(x =>
                    (x.City != null && x.City.Contains(location)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(location)));
            }

            if (minBudget.HasValue)
                query = query.Where(x =>
                    x.AverageMealPrice >= minBudget.Value);

            if (budget.HasValue)
                query = query.Where(x =>
                    x.AverageMealPrice <= budget.Value);

            restaurantCount = await query.CountAsync();

            restaurants = await ApplyOrdering(
                    query, sort, x => x.AverageMealPrice ?? 0)
                .ThenBy(x => x.DisplayOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // RESORTS
        if (all || category == "resorts")
        {
            var query = _context.Resorts
                .AsNoTracking()
                .Where(x => x.IsPublished && !x.IsArchived);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.CityOrRegion != null &&
                     x.CityOrRegion.Contains(keyword)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(keyword)));
            }

            if (location != null)
            {
                query = query.Where(x =>
                    (x.CityOrRegion != null &&
                     x.CityOrRegion.Contains(location)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(location)));
            }

            if (minBudget.HasValue)
                query = query.Where(x =>
                    x.StartingPricePerNight >= minBudget.Value);

            if (budget.HasValue)
                query = query.Where(x =>
                    x.StartingPricePerNight <= budget.Value);

            resortCount = await query.CountAsync();

            resorts = await ApplyOrdering(
                    query, sort, x => x.StartingPricePerNight ?? 0)
                .ThenBy(x => x.DisplayOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // TRANSPORT
        if (all || category == "transport")
        {
            var query = _context.TransportRoutes
                .AsNoTracking()
                .Where(x => x.IsPublished && x.IsActive && !x.IsArchived &&
                    x.VehicleLinks.Any(link => link.IsActive && !link.IsArchived &&
                        link.TransportService != null && link.TransportService.IsPublished && !link.TransportService.IsArchived));

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    x.StartingCity.Contains(keyword) ||
                    x.DestinationCity.Contains(keyword) ||
                    (x.Description != null && x.Description.Contains(keyword)));
            }

            if (location != null)
            {
                query = query.Where(x =>
                    x.StartingCity.Contains(location) ||
                    x.DestinationCity.Contains(location));
            }

            if (minBudget.HasValue)
                query = query.Where(x =>
                    x.StartingPricePKR >= minBudget.Value);

            if (budget.HasValue)
                query = query.Where(x =>
                    x.StartingPricePKR <= budget.Value);

            transportCount = await query.CountAsync();

            transport = await ApplyOrdering(
                    query, sort, x => x.StartingPricePKR)
                .ThenBy(x => x.DisplayOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // TOUR PACKAGES
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
                     x.DestinationName.Contains(keyword)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(keyword)));
            }

            if (location != null)
            {
                query = query.Where(x =>
                    (x.DestinationName != null &&
                     x.DestinationName.Contains(location)) ||
                    (x.Destination != null &&
                     x.Destination.Name.Contains(location)));
            }

            if (minBudget.HasValue)
                query = query.Where(x =>
                    x.PricePerAdult >= minBudget.Value);

            if (budget.HasValue)
                query = query.Where(x =>
                    x.PricePerAdult <= budget.Value);

            packageCount = await query.CountAsync();

            packages = await ApplyOrdering(
                    query, sort, x => x.PricePerAdult ?? 0)
                .ThenBy(x => x.DisplayOrder)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        ViewBag.Destinations = destinations;
        ViewBag.Hotels = hotels;
        ViewBag.Restaurants = restaurants;
        ViewBag.Resorts = resorts;
        ViewBag.Transport = transport;
        ViewBag.Packages = packages;

        ViewBag.DestinationCount = destinationCount;
        ViewBag.HotelCount = hotelCount;
        ViewBag.RestaurantCount = restaurantCount;
        ViewBag.ResortCount = resortCount;
        ViewBag.TransportCount = transportCount;
        ViewBag.PackageCount = packageCount;

        ViewBag.TotalResults =
            destinationCount +
            hotelCount +
            restaurantCount +
            resortCount +
            transportCount +
            packageCount;

        // Pagination applies independently to each displayed category.
        ViewBag.TotalPages = (int)Math.Ceiling(
            (double)ViewBag.TotalResults / PageSize);

        return View();
    }

    [HttpGet]

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static IOrderedQueryable<T> ApplyOrdering<T>(
        IQueryable<T> query,
        string sort,
        System.Linq.Expressions.Expression<Func<T, decimal>> price)
    {
        if (sort == "price-low")
            return query.OrderBy(price);

        if (sort == "price-high")
            return query.OrderByDescending(price);

        return query.OrderByDescending(x => EF.Property<int>(x, "DisplayOrder") == 0)
                    .ThenBy(x => EF.Property<int>(x, "DisplayOrder"));
    }
}
