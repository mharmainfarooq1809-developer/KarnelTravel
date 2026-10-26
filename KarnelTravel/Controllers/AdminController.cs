using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KarnelTravel.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // =========================================================
    // DASHBOARD
    // =========================================================

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public IActionResult Dashboard()
    {
        return View();
    }

    // =========================================================
    // DESTINATIONS - LIST
    // =========================================================

    [HttpGet("Destinations")]
    public async Task<IActionResult> Destinations()
    {
        var destinations = await _context.Destinations
            .AsNoTracking()
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .ToListAsync();

        return View(destinations);
    }

    // =========================================================
    // DESTINATIONS - CREATE FORM
    // =========================================================

    [HttpGet("Destinations/Create")]
    public IActionResult CreateDestination()
    {
        ViewData["Title"] = "New destination";
        ViewData["Section"] = "Destinations";

        return View("DestinationEditor", new Destination());
    }

    // =========================================================
    // DESTINATIONS - CREATE
    // =========================================================

    [HttpPost("Destinations/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDestination(
        Destination model,
        IFormFile? imageFile)
    {
        ViewData["Title"] = "New destination";
        ViewData["Section"] = "Destinations";

        model.Name = (model.Name ?? string.Empty).Trim();
        model.Slug = (model.Slug ?? string.Empty)
            .Trim()
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Destination name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Slug))
        {
            ModelState.AddModelError(
                nameof(model.Slug),
                "URL slug is required.");
        }

        if (!string.IsNullOrWhiteSpace(model.Slug) &&
            await _context.Destinations.AnyAsync(
                d => d.Slug == model.Slug))
        {
            ModelState.AddModelError(
                nameof(model.Slug),
                "This URL slug is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View("DestinationEditor", model);
        }

        if (imageFile != null && imageFile.Length > 0)
        {
            try
            {
                model.ImageUrl =
                    await SaveDestinationImageAsync(imageFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.ImageUrl),
                    ex.Message);

                return View("DestinationEditor", model);
            }
            catch (IOException)
            {
                ModelState.AddModelError(
                    nameof(model.ImageUrl),
                    "The image could not be saved. Please try again.");

                return View("DestinationEditor", model);
            }
        }

        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = null;

        _context.Destinations.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Destination created successfully.";

        return RedirectToAction(nameof(Destinations));
    }

    // =========================================================
    // DESTINATIONS - EDIT FORM
    // =========================================================

    [HttpGet("Destinations/Edit/{id:int}")]
    public async Task<IActionResult> EditDestination(int id)
    {
        var destination = await _context.Destinations.FindAsync(id);

        if (destination == null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Edit destination: {destination.Name}";
        ViewData["Section"] = "Destinations";

        return View("DestinationEditor", destination);
    }

    // =========================================================
    // DESTINATIONS - UPDATE
    // =========================================================

    [HttpPost("Destinations/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDestination(
        int id,
        Destination model,
        IFormFile? imageFile)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        ViewData["Title"] = "Edit destination";
        ViewData["Section"] = "Destinations";

        model.Name = (model.Name ?? string.Empty).Trim();
        model.Slug = (model.Slug ?? string.Empty)
            .Trim()
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Destination name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Slug))
        {
            ModelState.AddModelError(
                nameof(model.Slug),
                "URL slug is required.");
        }

        if (!string.IsNullOrWhiteSpace(model.Slug) &&
            await _context.Destinations.AnyAsync(
                d => d.Slug == model.Slug && d.Id != id))
        {
            ModelState.AddModelError(
                nameof(model.Slug),
                "This URL slug is already in use.");
        }

        var destination = await _context.Destinations.FindAsync(id);

        if (destination == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.ImageUrl = destination.ImageUrl;
            model.CreatedAt = destination.CreatedAt;
            model.UpdatedAt = destination.UpdatedAt;

            return View("DestinationEditor", model);
        }

        // Keep the existing image unless a new image is uploaded.
        if (imageFile != null && imageFile.Length > 0)
        {
            try
            {
                var newImagePath =
                    await SaveDestinationImageAsync(imageFile);

                if (newImagePath != null)
                {
                    destination.ImageUrl = newImagePath;
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.ImageUrl),
                    ex.Message);

                model.ImageUrl = destination.ImageUrl;
                model.CreatedAt = destination.CreatedAt;
                model.UpdatedAt = destination.UpdatedAt;

                return View("DestinationEditor", model);
            }
            catch (IOException)
            {
                ModelState.AddModelError(
                    nameof(model.ImageUrl),
                    "The image could not be saved. Please try again.");

                model.ImageUrl = destination.ImageUrl;
                model.CreatedAt = destination.CreatedAt;
                model.UpdatedAt = destination.UpdatedAt;

                return View("DestinationEditor", model);
            }
        }

        destination.Name = model.Name;
        destination.Slug = model.Slug;
        destination.Region = model.Region;
        destination.ShortDescription = model.ShortDescription;
        destination.Description = model.Description;
        destination.BestTimeToVisit = model.BestTimeToVisit;
        destination.IsPublished = model.IsPublished;
        destination.IsFeatured = model.IsFeatured;
        destination.DisplayOrder = model.DisplayOrder;
        destination.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Destination updated successfully.";

        return RedirectToAction(nameof(Destinations));
    }

    // =========================================================
    // DESTINATIONS - DELETE
    // =========================================================

    [HttpPost("Destinations/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDestination(int id)
    {
        var destination = await _context.Destinations.FindAsync(id);

        if (destination == null)
        {
            TempData["ErrorMessage"] = "Destination not found.";

            return RedirectToAction(nameof(Destinations));
        }

        _context.Destinations.Remove(destination);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Destination deleted successfully.";

        return RedirectToAction(nameof(Destinations));
    }

    // =========================================================
    // DESTINATIONS - IMAGE UPLOAD HELPER
    // =========================================================

    private async Task<string?> SaveDestinationImageAsync(
        IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return null;
        }

        const long maxFileSize = 5 * 1024 * 1024;

        if (imageFile.Length > maxFileSize)
        {
            throw new InvalidOperationException(
                "Image size must not exceed 5 MB.");
        }

        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            throw new IOException(
                "The website's wwwroot folder could not be located.");
        }

        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "destinations");

        Directory.CreateDirectory(uploadsFolder);

        var header = new byte[12];
        int bytesRead;

        await using (var input = imageFile.OpenReadStream())
        {
            bytesRead = await input.ReadAsync(
                header, 0, header.Length);
        }

        var isJpeg =
            bytesRead >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF;

        var isPng =
            bytesRead >= 8 &&
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A;

        var isWebp =
            bytesRead >= 12 &&
            header[0] == (byte)'R' &&
            header[1] == (byte)'I' &&
            header[2] == (byte)'F' &&
            header[3] == (byte)'F' &&
            header[8] == (byte)'W' &&
            header[9] == (byte)'E' &&
            header[10] == (byte)'B' &&
            header[11] == (byte)'P';

        string extension;

        if (isJpeg)
        {
            extension = ".jpg";
        }
        else if (isPng)
        {
            extension = ".png";
        }
        else if (isWebp)
        {
            extension = ".webp";
        }
        else
        {
            throw new InvalidOperationException(
                "Choose a valid JPG, PNG, or WebP image.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var output = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None))
        {
            await imageFile.CopyToAsync(output);
        }

        return $"/uploads/destinations/{fileName}";
    }

    // =========================================================
    // PROPERTIES - DATABASE-BACKED LIST
    // =========================================================

    [HttpGet("Properties")]
    public async Task<IActionResult> Properties()
    {
        ViewData["Title"] = "Stays & properties";
        ViewData["Section"] = "Stays";

        var hotels = await _context.Hotels
            .AsNoTracking()
            .Select(h => new AdminPropertyRowViewModel
            {
                Id = h.Id,
                Name = h.Name,
                Category = "Hotel",
                Location = h.City ?? "",
                ImageUrl = h.ImageUrl,
                Price = h.StartingPricePerNight,
                IsPublished = h.IsPublished,
                IsFeatured = h.IsFeatured,
                DestinationName = h.Destination != null
                    ? h.Destination.Name
                    : null
            })
            .ToListAsync();

        var restaurants = await _context.Restaurants
            .AsNoTracking()
            .Select(r => new AdminPropertyRowViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Category = "Restaurant",
                Location = r.City ?? "",
                ImageUrl = r.ImageUrl,
                Price = r.AverageMealPrice,
                IsPublished = r.IsPublished,
                IsFeatured = r.IsFeatured,
                DestinationName = r.Destination != null
                    ? r.Destination.Name
                    : null
            })
            .ToListAsync();

        var resorts = await _context.Resorts
            .AsNoTracking()
            .Select(r => new AdminPropertyRowViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Category = "Resort",
                Location = r.CityOrRegion ?? "",
                ImageUrl = r.ImageUrl,
                Price = r.StartingPricePerNight,
                IsPublished = r.IsPublished,
                IsFeatured = r.IsFeatured,
                DestinationName = r.Destination != null
                    ? r.Destination.Name
                    : null
            })
            .ToListAsync();

        var allProperties = hotels
            .Concat(restaurants)
            .Concat(resorts)
            .OrderBy(p => p.Name)
            .ToList();

        var model = new AdminPropertiesViewModel
        {
            Properties = allProperties,
            HotelCount = hotels.Count,
            RestaurantCount = restaurants.Count,
            ResortCount = resorts.Count,
            PublishedCount = allProperties.Count(p => p.IsPublished),
            DraftCount = allProperties.Count(p => !p.IsPublished)
        };

        return View(model);
    }

    // =========================================================
    // TOURS - PLACEHOLDER
    // =========================================================

    [HttpGet("Tours")]
    public IActionResult Tours()
    {
        ViewData["Title"] = "Experiences";
        ViewData["Section"] = "Experiences";

        return View("ComingSoon");
    }

    [HttpGet("Tours/Create")]
    public IActionResult CreateTour()
    {
        ViewData["Title"] = "New tour package";
        ViewData["Section"] = "Experiences";

        return View("TourEditor");
    }

    [HttpGet("Tours/Edit/{id?}")]
    public IActionResult EditTour(string? id)
    {
        ViewData["Title"] = string.IsNullOrEmpty(id)
            ? "Edit tour"
            : $"Edit tour #{id}";

        ViewData["Section"] = "Experiences";

        return View("TourEditor");
    }

    // =========================================================
    // BOOKINGS
    // =========================================================

    [HttpGet("Bookings")]
    public IActionResult Bookings()
    {
        return View();
    }

    // =========================================================
    // TRANSPORT - PLACEHOLDER
    // =========================================================

    [HttpGet("Transport")]
    public IActionResult Transport()
    {
        ViewData["Title"] = "Transport & scheduling";
        ViewData["Section"] = "Transport";

        return View("ComingSoon");
    }

    // =========================================================
    // OTHER ADMIN PAGES - PLACEHOLDERS
    // =========================================================

    [HttpGet("Calendar")]
    public IActionResult Calendar()
    {
        ViewData["Title"] = "Calendar";
        ViewData["Section"] = "Calendar";

        return View("ComingSoon");
    }

    [HttpGet("Inquiries")]
    public IActionResult Inquiries()
    {
        ViewData["Title"] = "Inquiries";
        ViewData["Section"] = "Inquiries";

        return View("ComingSoon");
    }

    [HttpGet("Reviews")]
    public IActionResult Reviews()
    {
        ViewData["Title"] = "Reviews";
        ViewData["Section"] = "Reviews";

        return View("ComingSoon");
    }

    [HttpGet("WebsiteContent")]
    public IActionResult WebsiteContent()
    {
        ViewData["Title"] = "Website content";
        ViewData["Section"] = "Website content";

        return View("ComingSoon");
    }

    [HttpGet("Reports")]
    public IActionResult Reports()
    {
        ViewData["Title"] = "Reports & analytics";
        ViewData["Section"] = "Reports";

        return View("ComingSoon");
    }

    [HttpGet("Settings")]
    public IActionResult Settings()
    {
        ViewData["Title"] = "Settings";
        ViewData["Section"] = "Settings";

        return View("ComingSoon");
    }
}