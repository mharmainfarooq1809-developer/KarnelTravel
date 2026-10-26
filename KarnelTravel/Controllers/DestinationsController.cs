
using KarnelTravel.Data;
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

            return View("~/Views/Destinations/Details.cshtml", destination);
        }
    }
}
