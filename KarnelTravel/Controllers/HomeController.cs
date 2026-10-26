using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KarnelTravel.Data; // adjust to your namespace
using KarnelTravel.Models;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.FeaturedDestinations = await _db.Destinations
            .Where(d => d.IsPublished && d.IsFeatured)   // adjust to your flags
            .OrderBy(d => d.Name)
            .Take(3)                                     // matches the 3-card feature grid
            .AsNoTracking()
            .ToListAsync();

        return View();
    }
}