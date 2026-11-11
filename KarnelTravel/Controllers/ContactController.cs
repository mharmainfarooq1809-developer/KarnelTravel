
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
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
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string? category = null, string? service = null)
        {
            var settings = await _settingsService.GetAsync();
            if (!settings.AllowContactSubmissions)
                return NotFound();

            ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
            var model = new ContactInquiry();
        if (IsExperiencePlanning(category))
            model.Category = "Experience Planning";
        else if (!string.IsNullOrWhiteSpace(category))
            model.Category = category.Trim();
            if (!string.IsNullOrWhiteSpace(service))
                model.Message = $"I would like to plan a trip to {service.Trim()}.";

        ViewBag.IsExperiencePlanning = IsExperiencePlanning(model.Category);
        ViewBag.ContactHeading = ViewBag.IsExperiencePlanning ? "Plan Your Experience" : "Send an inquiry";

            return View(model);
        }

        // Save contact inquiry
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactInquiry model)
        {
            var settings = await _settingsService.GetAsync();
            if (!settings.AllowContactSubmissions)
                return BadRequest("Contact submissions are currently unavailable.");

        var isExperiencePlanning = IsExperiencePlanning(model.Category);
        if (isExperiencePlanning)
        {
            model.Category = "Experience Planning";
            if (string.IsNullOrWhiteSpace(model.PreferredExperience))
                ModelState.AddModelError(nameof(model.PreferredExperience), "Tell us which experience you would like to plan.");
            if (string.IsNullOrWhiteSpace(model.Destination))
                ModelState.AddModelError(nameof(model.Destination), "Enter the destination for your experience.");
            if (!model.TravelStartDate.HasValue)
                ModelState.AddModelError(nameof(model.TravelStartDate), "Select a travel start date.");
            if (!model.TravelEndDate.HasValue)
                ModelState.AddModelError(nameof(model.TravelEndDate), "Select a travel end date.");
            if (!model.Travellers.HasValue)
                ModelState.AddModelError(nameof(model.Travellers), "Enter the number of travellers.");
            if (model.TravelStartDate.HasValue && model.TravelStartDate.Value.Date < DateTime.Today)
                ModelState.AddModelError(nameof(model.TravelStartDate), "The travel start date cannot be in the past.");
            if (model.TravelStartDate.HasValue && model.TravelEndDate.HasValue && model.TravelEndDate < model.TravelStartDate)
                ModelState.AddModelError(nameof(model.TravelEndDate), "The end date must be on or after the start date.");
        }

            if (!ModelState.IsValid)
            {
                ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
            ViewBag.IsExperiencePlanning = isExperiencePlanning;
            ViewBag.ContactHeading = isExperiencePlanning ? "Plan Your Experience" : "Send an inquiry";
                return View(model);
            }

            model.Id = 0;
            model.Name = model.Name.Trim();
            model.Email = model.Email.Trim();
            model.Phone = model.Phone?.Trim();
            model.Category = model.Category.Trim();
            model.Message = model.Message.Trim();

        if (isExperiencePlanning)
        {
            var experienceDetails = string.Join("\n", new[]
            {
                $"Preferred experience: {model.PreferredExperience?.Trim() ?? "Not specified"}",
                $"Destination: {model.Destination?.Trim() ?? "Not specified"}",
                $"Travel dates: {FormatDate(model.TravelStartDate)} to {FormatDate(model.TravelEndDate)}",
                $"Travellers: {model.Travellers?.ToString() ?? "Not specified"}",
                $"Additional requirements: {model.Message}"
            });

            if (experienceDetails.Length > 2000)
            {
                ModelState.AddModelError(nameof(model.Message), "Please shorten the additional requirements to 2,000 characters or fewer.");
                ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
                ViewBag.IsExperiencePlanning = true;
                ViewBag.ContactHeading = "Plan Your Experience";
                return View(model);
            }

            model.Message = experienceDetails;
        }

            model.SubmittedAt = DateTime.UtcNow;
            model.Status = "New";
            model.AdminNotes = null;

            _db.ContactInquiries.Add(model);

            await _db.SaveChangesAsync();

            TempData["ContactSuccess"] =
                "Thank you — your inquiry has been received. We will contact you soon.";

            return RedirectToAction(nameof(Index));
        }

        static bool IsExperiencePlanning(string? value) =>
            string.Equals(value?.Trim(), "Experience planning", StringComparison.OrdinalIgnoreCase);

        static string FormatDate(DateTime? value) =>
            value.HasValue ? value.Value.ToString("dd MMM yyyy") : "Not specified";
    }
}
