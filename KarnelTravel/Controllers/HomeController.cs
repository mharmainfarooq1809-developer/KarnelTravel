using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KarnelTravel.Data; // adjust to your namespace
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authorization;

[Authorize]
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

    public async Task<IActionResult> Index()
    {
        ViewBag.FeaturedDestinations = await _db.Destinations
            .Where(d => d.IsPublished && d.IsFeatured)   // adjust to your flags
            .OrderBy(d => d.Name)
            .Take(3)                                     // matches the 3-card feature grid
            .AsNoTracking()
            .ToListAsync();

        ViewBag.WebsiteContent = await _websiteContentService.GetAsync();

        return View();
    }
}