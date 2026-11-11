
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class ResortsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResortsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // RESORT LISTING
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var resorts = await _context.Resorts
                .AsNoTracking()
                .Where(r => r.IsPublished)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.Name)
                .ToListAsync();

            return View(
                "~/Views/Information/Dropdown/Resorts.cshtml",
                resorts
            );
        }

        // RESORT DETAILS
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var resort = await _context.Resorts
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Id == id && r.IsPublished);

            if (resort == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r =>
                    r.ResortId == resort.Id &&
                    r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;
            ViewBag.TotalReviews = reviews.Count;

            ViewBag.AverageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0;

            return View("~/Views/Resorts/Details.cshtml", resort);
        }
    }
}
