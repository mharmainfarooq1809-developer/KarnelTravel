
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebsiteContentService _websiteContentService;
        private readonly ISettingsService _settingsService;

        public ContactController(
            ApplicationDbContext db,
            IWebsiteContentService websiteContentService,
            ISettingsService settingsService)
        {
            _db = db;
            _websiteContentService = websiteContentService;
            _settingsService = settingsService;
        }

        // Display contact form
        [HttpGet]
        public async Task<IActionResult> Index(string? category = null, string? service = null)
        {
            var settings = await _settingsService.GetAsync();
            if (!settings.AllowContactSubmissions)
                return NotFound();

            ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
            var model = new ContactInquiry();
            if (!string.IsNullOrWhiteSpace(category))
                model.Category = category.Trim();
            if (!string.IsNullOrWhiteSpace(service))
                model.Message = $"I would like to plan a trip to {service.Trim()}.";

            return View(model);
        }

        // Save contact inquiry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactInquiry model)
        {
            var settings = await _settingsService.GetAsync();
            if (!settings.AllowContactSubmissions)
                return BadRequest("Contact submissions are currently unavailable.");

            if (!ModelState.IsValid)
            {
                ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
                return View(model);
            }

            model.Id = 0;
            model.Name = model.Name.Trim();
            model.Email = model.Email.Trim();
            model.Phone = model.Phone?.Trim();
            model.Category = model.Category.Trim();
            model.Message = model.Message.Trim();

            model.SubmittedAt = DateTime.UtcNow;
            model.Status = "New";
            model.AdminNotes = null;

            _db.ContactInquiries.Add(model);

            await _db.SaveChangesAsync();

            TempData["ContactSuccess"] =
                "Thank you — your inquiry has been received. We will contact you soon.";

            return RedirectToAction(nameof(Index));
        }
    }
}
