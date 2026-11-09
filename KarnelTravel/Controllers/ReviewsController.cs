using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISettingsService _settingsService;

        public ReviewsController(ApplicationDbContext context, ISettingsService settingsService)
        {
            _context = context;
            _settingsService = settingsService;
        }

        // GET: /Reviews/Create?type=hotel&id=1
        [HttpGet]
        public async Task<IActionResult> Create(string type, int id)
        {
            var settings = await _settingsService.GetAsync();
            if (!settings.AllowFeedbackSubmissions)
                return NotFound();

            if (id <= 0)
                return NotFound();

            var listingName = await GetPublishedListingName(type, id);

            if (listingName == null)
                return NotFound();

            var model = new ReviewSubmissionViewModel
            {
                ListingType = NormalizeType(type),
                ListingId = id,
                ListingName = listingName
            };

            ViewBag.SuccessMessage = TempData["ReviewSuccess"];

            return View(model);
        }

        // POST: /Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ReviewSubmissionViewModel model)
        {
            var settings = await _settingsService.GetAsync();
            if (!settings.AllowFeedbackSubmissions)
                return BadRequest("Feedback submissions are currently unavailable.");

            var type = NormalizeType(model.ListingType);

            var listingName = await GetPublishedListingName(
                type,
                model.ListingId
            );

            if (listingName == null)
                return NotFound();

            model.ListingType = type;
            model.ListingName = listingName;

            if (!ModelState.IsValid)
                return View(model);

            var review = new Review
            {
                ReviewerName = model.ReviewerName.Trim(),
                Title = model.Title.Trim(),
                Comment = model.Comment.Trim(),
                Rating = model.Rating,
                CreatedAt = DateTime.UtcNow,
                Status = ReviewStatus.Pending
            };

            // Attach the review to exactly one listing.
            switch (type)
            {
                case "destination":
                    review.DestinationId = model.ListingId;
                    break;

                case "hotel":
                    review.HotelId = model.ListingId;
                    break;

                case "restaurant":
                    review.RestaurantId = model.ListingId;
                    break;

                case "resort":
                    review.ResortId = model.ListingId;
                    break;

                case "transport":
                    review.TransportServiceId = model.ListingId;
                    break;

                case "tourpackage":
                    review.TourPackageId = model.ListingId;
                    break;

                default:
                    return NotFound();
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] =
                "Your review has been submitted successfully. It will appear after approval.";

            return RedirectToAction(
                nameof(Create),
                new { type, id = model.ListingId }
            );
        }

        private async Task<string?> GetPublishedListingName(
            string type,
            int id)
        {
            switch (NormalizeType(type))
            {
                case "destination":
                    return await _context.Destinations
                        .Where(x => x.Id == id && x.IsPublished)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                case "hotel":
                    return await _context.Hotels
                        .Where(x => x.Id == id && x.IsPublished)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                case "restaurant":
                    return await _context.Restaurants
                        .Where(x => x.Id == id && x.IsPublished)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                case "resort":
                    return await _context.Resorts
                        .Where(x => x.Id == id && x.IsPublished)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                case "transport":
                    return await _context.TransportServices
                        .Where(x => x.Id == id && x.IsPublished && !x.IsArchived)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                case "tourpackage":
                    return await _context.TourPackages
                        .Where(x => x.Id == id && x.IsPublished)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();

                default:
                    return null;
            }
        }

        private static string NormalizeType(string? type)
        {
            return (type ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Replace("-", "")
                .Replace("_", "")
                .Replace(" ", "");
        }
    }
}