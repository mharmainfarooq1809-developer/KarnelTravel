using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KarnelTravel.Data; // adjust to your namespace
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authorization;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebsiteContentService _websiteContentService;

    public HomeController(
        ApplicationDbContext db,
        IWebsiteContentService websiteContentService)
    {
        _db = db;
        _websiteContentService = websiteContentService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        ViewBag.FeaturedDestinations = await _db.Destinations
            .Where(d => d.IsPublished && d.IsFeatured)   // adjust to your flags
            .OrderBy(d => d.Name)
            .Take(3)                                     // matches the 3-card feature grid
            .AsNoTracking()
            .ToListAsync();

        ViewBag.FeaturedPackages = await _db.TourPackages
            .Where(x => x.IsPublished)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.FeaturedHotels = await _db.Hotels
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.FeaturedResorts = await _db.Resorts
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.FeaturedRestaurants = await _db.Restaurants
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.FeaturedTransport = await _db.TransportServices
            .Where(x => x.IsPublished && !x.IsArchived)
            .OrderBy(x => x.Name)
            .Take(3)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.WebsiteContent = await _websiteContentService.GetAsync();

        return View();
    }
}