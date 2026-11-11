using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class TransportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TRANSPORT LISTING
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var routes = await _context.TransportRoutes
                .AsNoTracking()
                .Where(t => t.IsPublished && t.IsActive && !t.IsArchived &&
                    t.VehicleLinks.Any(x => x.IsActive && !x.IsArchived && x.TransportService != null && x.TransportService.IsPublished && !x.TransportService.IsArchived))
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(
                "~/Views/Information/Dropdown/Transport.cshtml",
                routes
            );
        }

        // TRANSPORT DETAILS
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var route = await _context.TransportRoutes
                .AsNoTracking()
                .Include(x => x.VehicleLinks)
                .ThenInclude(x => x.TransportService)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished && x.IsActive && !x.IsArchived);

            if (route == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r =>
                    route.VehicleLinks.Select(x => x.TransportServiceId).Contains(r.TransportServiceId ?? 0) &&
                    r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var vehicles = route.VehicleLinks
                .Where(x => x.IsActive && !x.IsArchived && x.TransportService != null && x.TransportService.IsPublished && !x.TransportService.IsArchived)
                .Select(x => x.TransportService!)
                .ToList();

            ViewBag.Reviews = reviews;
            ViewBag.TotalReviews = reviews.Count;

            ViewBag.AverageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0;
            ViewBag.Vehicles = vehicles;

            return View(
                "~/Views/Transport/Details.cshtml",
                route
            );
        }
    }
}
