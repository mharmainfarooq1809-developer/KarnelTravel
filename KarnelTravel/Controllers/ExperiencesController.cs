using KarnelTravel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class ExperiencesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ExperiencesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? category)
        {
            var query = _db.Experiences
                .AsNoTracking()
                .Where(x => x.IsPublished);

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All Experiences", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => x.Category == category.Trim());

            var experiences = await query
                .OrderByDescending(x => x.IsFeatured)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return View(experiences);
        }

        [HttpGet("Experiences/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var experience = await _db.Experiences
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished);

            return experience is null ? NotFound() : View(experience);
        }
    }
}