
using KarnelTravel.Data;
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
    }
}
