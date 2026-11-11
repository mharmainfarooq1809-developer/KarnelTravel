
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // RESTAURANT LISTING
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var restaurants = await _context.Restaurants
                .AsNoTracking()
                .Where(r => r.IsPublished)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.Name)
                .ToListAsync();

            return View(
                "~/Views/Information/Dropdown/Resturants.cshtml",
                restaurants
            );
        }

        // RESTAURANT DETAILS
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Id == id && r.IsPublished);

            if (restaurant == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r =>
                    r.RestaurantId == restaurant.Id &&
                    r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Reviews = reviews;
            ViewBag.TotalReviews = reviews.Count;

            ViewBag.AverageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0;

            return View(
                "~/Views/Restaurants/Details.cshtml",
                restaurant
            );
        }
    }
}
