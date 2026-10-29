
using KarnelTravel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class InformationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InformationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/destinations")]
        public async Task<IActionResult> Destination()
        {
            var destinations = await _context.Destinations
                .AsNoTracking()
                .Where(d => d.IsPublished)
                .OrderBy(d => d.DisplayOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();

            return View("Dropdown/destination", destinations);
        }

        [HttpGet("/transport")]
        public IActionResult Transport()
        {
            return View("Dropdown/transport");
        }

        [HttpGet("/hotels")]
        public async Task<IActionResult> Hotels()
        {
            var hotels = await _context.Hotels
                .AsNoTracking()
                .Where(h => h.IsPublished)
                .OrderBy(h => h.DisplayOrder)
                .ThenBy(h => h.Name)
                .ToListAsync();

            return View("Dropdown/Hotels", hotels);
        }
    }
}
