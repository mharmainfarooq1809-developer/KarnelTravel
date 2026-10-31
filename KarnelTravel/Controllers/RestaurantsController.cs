
using KarnelTravel.Data;
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

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.IsPublished);

            if (restaurant == null)
            {
                return NotFound();
            }

            return View("~/Views/Restaurants/Details.cshtml", restaurant);
        }
    }
}
