
using KarnelTravel.Data;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DestinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Destinations/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var destination = await _context.Destinations
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.Slug == slug && d.IsPublished);

            if (destination == null)
            {
                return NotFound();
            }

            var model = new DestinationDetailsViewModel
            {
                Destination = destination,

                Hotels = await _context.Hotels
                    .AsNoTracking()
                    .Where(h =>
                        h.DestinationId == destination.Id &&
                        h.IsPublished)
                    .OrderBy(h => h.DisplayOrder)
                    .ThenBy(h => h.Name)
                    .ToListAsync(),

                Restaurants = await _context.Restaurants
                    .AsNoTracking()
                    .Where(r =>
                        r.DestinationId == destination.Id &&
                        r.IsPublished)
                    .OrderBy(r => r.DisplayOrder)
                    .ThenBy(r => r.Name)
                    .ToListAsync(),

                Resorts = await _context.Resorts
                    .AsNoTracking()
                    .Where(r =>
                        r.DestinationId == destination.Id &&
                        r.IsPublished)
                    .OrderBy(r => r.DisplayOrder)
                    .ThenBy(r => r.Name)
                    .ToListAsync()
            };

            return View(
                "~/Views/Destinations/Details.cshtml",
                model);
        }
    }
}
