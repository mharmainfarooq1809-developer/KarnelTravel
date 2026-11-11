
using KarnelTravel.Data;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/policies/terms-and-conditions")]
        public IActionResult TermsAndConditions() => View("Policies/TermsAndConditions");

        [AllowAnonymous]
        [HttpGet("/policies/privacy-policy")]
        public IActionResult PrivacyPolicy() => View("Policies/PrivacyPolicy");

        [AllowAnonymous]
        [HttpGet("/policies/booking-cancellation")]
        public IActionResult BookingCancellation() => View("Policies/BookingCancellation");

        [AllowAnonymous]
        [HttpGet("/policies/travel-disclaimer")]
        public IActionResult TravelDisclaimer() => View("Policies/TravelDisclaimer");

        [AllowAnonymous]
        [HttpGet("/policies/{slug}")]
        public IActionResult UnknownPolicy(string slug) => NotFound();

        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpGet("/transport")]
        public async Task<IActionResult> Transport()
        {
            var transportRoutes = await _context.TransportRoutes
                .AsNoTracking()
                .Where(t => t.IsPublished && t.IsActive && !t.IsArchived &&
                    t.VehicleLinks.Any(x => x.IsActive && !x.IsArchived &&
                        x.TransportService != null && x.TransportService.IsPublished && !x.TransportService.IsArchived))
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(
                "~/Views/Information/Dropdown/transport.cshtml",
                transportRoutes
            );
        }

        [AllowAnonymous]
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
