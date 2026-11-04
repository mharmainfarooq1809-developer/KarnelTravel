
using KarnelTravel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class ToursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Tours
        public async Task<IActionResult> Index()
        {
            var packages = await _context.TourPackages
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(packages);
        }

        // GET: /Tours/Details/hunza-valley-explorer
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var package = await _context.TourPackages
                .AsNoTracking()
                .Include(p => p.ItineraryDays)
                .Include(p => p.Departures)
                .FirstOrDefaultAsync(p =>
                    p.Slug == slug && p.IsPublished);

            if (package == null)
                return NotFound();

            return View(package);
        }
    }
}
