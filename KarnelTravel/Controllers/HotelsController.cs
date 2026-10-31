using KarnelTravel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class HotelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HotelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // HOTEL LISTING
        public async Task<IActionResult> Index()
        {
            var hotels = await _context.Hotels
                .AsNoTracking()
                .Where(h => h.IsPublished)
                .OrderBy(h => h.DisplayOrder)
                .ThenBy(h => h.Name)
                .ToListAsync();

            return View("~/Views/Information/Dropdown/Hotels.cshtml", hotels);
        }

        // HOTEL DETAILS
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var hotel = await _context.Hotels
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id && h.IsPublished);

            if (hotel == null)
            {
                return NotFound();
            }

            return View("~/Views/Hotels/Details.cshtml", hotel);
        }
    }
}