
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpGet("/Destinations/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var requestedSlug = slug.Trim();
            var normalizedSlug = requestedSlug.Replace(' ', '-');
            var destination = await _context.Destinations
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.IsPublished &&
                    (d.Slug == requestedSlug ||
                     d.Slug == normalizedSlug ||
                     d.Name == requestedSlug));

            if (destination == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r =>
                    r.DestinationId == destination.Id &&
                    r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

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

                Packages = await _context.TourPackages
                    .AsNoTracking()
                    .Include(p => p.Departures)
                    .Where(p => p.DestinationId == destination.Id && p.IsPublished)
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Name)
                    .ToListAsync(),

                Resorts = await _context.Resorts
                    .AsNoTracking()
                    .Where(r =>
                        r.DestinationId == destination.Id &&
                        r.IsPublished)
                    .OrderBy(r => r.DisplayOrder)
                    .ThenBy(r => r.Name)
                    .ToListAsync(),

                Reviews = reviews,

                TotalReviews = reviews.Count,

                AverageRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 1)
                    : 0
            };

            return View(
                "~/Views/Destinations/Details.cshtml",
                model);
        }
    }
}
