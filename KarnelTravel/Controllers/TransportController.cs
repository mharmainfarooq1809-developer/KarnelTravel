
using KarnelTravel.Data;
using KarnelTravel.Models;
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
        public async Task<IActionResult> Index()
        {
            var transportServices = await _context.TransportServices
                .AsNoTracking()
                .Where(t => t.IsPublished)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(
                "~/Views/Information/Dropdown/Transport.cshtml",
                transportServices
            );
        }

        // TRANSPORT DETAILS
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var transport = await _context.TransportServices
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == id && t.IsPublished);

            if (transport == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r =>
                    r.TransportServiceId == transport.Id &&
                    r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var routes = await _context.TransportRoutePricings
                .AsNoTracking()
                .Where(route => route.TransportServiceId == transport.Id && route.IsActive)
                .OrderBy(route => route.PickupCity)
                .ThenBy(route => route.DropoffCity)
                .ToListAsync();

            ViewBag.Reviews = reviews;
            ViewBag.TotalReviews = reviews.Count;

            ViewBag.AverageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0;
            ViewBag.Routes = routes;

            return View(
                "~/Views/Transport/Details.cshtml",
                transport
            );
        }
    }
}
