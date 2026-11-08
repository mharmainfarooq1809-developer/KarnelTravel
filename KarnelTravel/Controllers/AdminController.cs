using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Claims;


namespace KarnelTravel.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IWebsiteContentService _websiteContentService;
    private readonly ISettingsService _settingsService;
    private readonly IAuditLogService _auditLogService;
    private readonly IBookingStatusService _bookingStatusService;

    public AdminController(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        IWebsiteContentService websiteContentService,
        ISettingsService settingsService,
        IAuditLogService auditLogService,
        IBookingStatusService bookingStatusService)
    {
        _context = context;
        _environment = environment;
        _websiteContentService = websiteContentService;
        _settingsService = settingsService;
        _auditLogService = auditLogService;
        _bookingStatusService = bookingStatusService;
    }

    [HttpGet("Experiences")]
    public async Task<IActionResult> Experiences()
    {
        var model = await _context.Experiences.AsNoTracking()
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new AdminExperienceRowViewModel
            {
                Id = x.Id, Name = x.Name, Category = x.Category, Location = x.Location,
                StartingPrice = x.StartingPrice, IsPublished = x.IsPublished,
                IsFeatured = x.IsFeatured, DisplayOrder = x.DisplayOrder
            }).ToListAsync();
        return View(model);
    }

    [HttpGet("Experiences/Create")]
    public IActionResult CreateExperience() => View("ExperienceEditor", new AdminExperienceFormViewModel { IsPublished = true });

    [HttpPost("Experiences/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExperience(AdminExperienceFormViewModel model)
    {
        if (!ModelState.IsValid) return View("ExperienceEditor", model);
        _context.Experiences.Add(ToExperience(model));
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Experience created successfully.";
        return RedirectToAction(nameof(Experiences));
    }

    [HttpGet("Experiences/Edit/{id:int}")]
    public async Task<IActionResult> EditExperience(int id)
    {
        var experience = await _context.Experiences.FindAsync(id);
        return experience is null ? NotFound() : View("ExperienceEditor", AdminExperienceFormViewModel.FromEntity(experience));
    }

    [HttpPost("Experiences/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExperience(int id, AdminExperienceFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("ExperienceEditor", model);
        var experience = await _context.Experiences.FindAsync(id);
        if (experience is null) return NotFound();
        ApplyExperience(experience, model);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Experience updated successfully.";
        return RedirectToAction(nameof(Experiences));
    }

    [HttpPost("Experiences/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExperience(int id)
    {
        var experience = await _context.Experiences.FindAsync(id);
        if (experience is null) return NotFound();
        if (await _context.Bookings.AnyAsync(x => x.ServiceType == "Experience" && x.ServiceName == experience.Name))
        {
            TempData["ErrorMessage"] = "This experience cannot be deleted because it has booking records. Unpublish it instead.";
            return RedirectToAction(nameof(Experiences));
        }
        _context.Experiences.Remove(experience);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Experience deleted successfully.";
        return RedirectToAction(nameof(Experiences));
    }

    private static Experience ToExperience(AdminExperienceFormViewModel model)
    {
        var entity = new Experience();
        ApplyExperience(entity, model);
        return entity;
    }

    private static void ApplyExperience(Experience entity, AdminExperienceFormViewModel model)
    {
        entity.Name = model.Name.Trim(); entity.Category = model.Category.Trim(); entity.Location = model.Location.Trim();
        entity.Duration = model.Duration.Trim(); entity.ImageUrl = model.ImageUrl?.Trim();
        entity.ShortDescription = model.ShortDescription?.Trim(); entity.Description = model.Description?.Trim();
        entity.StartingPrice = model.StartingPrice; entity.Includes = model.Includes?.Trim(); entity.Excludes = model.Excludes?.Trim();
        entity.IsPublished = model.IsPublished; entity.IsFeatured = model.IsFeatured; entity.DisplayOrder = model.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;
    }


    // =========================================================
    // DASHBOARD
    // =========================================================

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var model = new AdminDashboardViewModel
        {
            TotalBookings = await _context.Bookings
                .AsNoTracking()
                .CountAsync(),

            TotalTripInquiries = await _context.TripInquiries
                .AsNoTracking()
                .CountAsync(),

            TotalContactInquiries = await _context.ContactInquiries
                .AsNoTracking()
                .CountAsync(),

            TotalListings =
                await _context.Destinations.AsNoTracking().CountAsync()
                + await _context.Hotels.AsNoTracking().CountAsync()
                + await _context.Restaurants.AsNoTracking().CountAsync()
                + await _context.Resorts.AsNoTracking().CountAsync()
                + await _context.TourPackages.AsNoTracking().CountAsync(),

            TotalReviews = await _context.Reviews
                .AsNoTracking()
                .CountAsync(),

            RecentBookings = await _context.Bookings
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new RecentBookingViewModel
                {
                    ReferenceNumber = x.ReferenceNumber,
                    CustomerName = x.CustomerName,
                    ServiceName = x.ServiceName,
                    BookingDate = x.BookingDate,
                    Adults = x.Adults,
                    Children = x.Children,
                    Status = x.Status
                })
                .ToListAsync()
        };

        return View(model);
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
        await _auditLogService.RecordAsync(User.Identity?.Name, "Listing created", $"Destination {model.Name} was created.", "Destination", model.Id.ToString());

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
        await _auditLogService.RecordAsync(User.Identity?.Name, "Listing updated", $"Destination {destination.Name} was updated.", "Destination", destination.Id.ToString());

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

        try
        {
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Destination deleted successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "This destination could not be deleted. " +
                "It may be linked to existing properties.";
        }

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
    // PROPERTIES - LIST
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
    // PROPERTIES - CREATE FORM
    // =========================================================

    [HttpGet("Properties/Create")]
    public IActionResult CreateProperty()
    {
        ViewData["Title"] = "Add Property";
        ViewData["Section"] = "Stays";
        ViewBag.Destinations = _context.Destinations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToList();

        return View("PropertyForm", new AdminPropertyFormViewModel());
    }

    // =========================================================
    // PROPERTIES - CREATE
    // =========================================================

    [HttpPost("Properties/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProperty(
        AdminPropertyFormViewModel model)
    {
        ViewData["Title"] = "Add Property";
        ViewData["Section"] = "Stays";
        ViewBag.Destinations = await GetPropertyDestinationsAsync();

        if (!ValidatePropertyForm(model))
        {
            return View("PropertyForm", model);
        }

        if (!await ValidatePropertyDestinationAsync(model))
            return View("PropertyForm", model);

        var slug = CreatePropertySlug(model.Name);

        if (await PropertySlugExistsAsync(model.Category, slug))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "A property with this name already exists in this category.");

            return View("PropertyForm", model);
        }

        var now = DateTime.UtcNow;

        switch (model.Category)
        {
            case "Hotel":
                _context.Hotels.Add(new Hotel
                {
                    Name = model.Name.Trim(),
                    Slug = slug,
                    City = model.Location.Trim(),
                    Address = model.Address.Trim(),
                    ShortDescription = model.ShortDescription.Trim(),
                    Description = model.Description.Trim(),
                    ImageUrl = model.ImageUrl.Trim(),
                    StartingPricePerNight = model.Price,
                    StarRating = model.StarRating,
                    ContactPhone = model.ContactPhone.Trim(),
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    DestinationId = model.DestinationId,
                    CreatedAt = now
                });
                break;

            case "Restaurant":
                _context.Restaurants.Add(new Restaurant
                {
                    Name = model.Name.Trim(),
                    Slug = slug,
                    City = model.Location.Trim(),
                    Address = model.Address.Trim(),
                    ShortDescription = model.ShortDescription.Trim(),
                    Description = model.Description.Trim(),
                    ImageUrl = model.ImageUrl.Trim(),
                    AverageMealPrice = model.Price,
                    CuisineType = model.CuisineType!.Trim(),
                    ContactPhone = model.ContactPhone.Trim(),
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    DestinationId = model.DestinationId,
                    CreatedAt = now
                });
                break;

            case "Resort":
                _context.Resorts.Add(new Resort
                {
                    Name = model.Name.Trim(),
                    Slug = slug,
                    CityOrRegion = model.Location.Trim(),
                    Address = model.Address.Trim(),
                    ShortDescription = model.ShortDescription.Trim(),
                    Description = model.Description.Trim(),
                    ImageUrl = model.ImageUrl.Trim(),
                    StartingPricePerNight = model.Price,
                    ContactPhone = model.ContactPhone.Trim(),
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    DestinationId = model.DestinationId,
                    CreatedAt = now
                });
                break;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.DestinationId), "The selected destination is no longer available. Select an existing destination and try again.");
            return View("PropertyForm", model);
        }

        TempData["SuccessMessage"] =
            $"{model.Category} added successfully.";

        return RedirectToAction(nameof(Properties));
    }

    // =========================================================
    // PROPERTIES - EDIT FORM
    // =========================================================

    [HttpGet("Properties/Edit/{id:int}")]
    public async Task<IActionResult> EditProperty(
        int id,
        string category)
    {
        category = (category ?? string.Empty).Trim();

        AdminPropertyFormViewModel model;

        switch (category)
        {
            case "Hotel":
                {
                    var item = await _context.Hotels
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (item == null)
                        return NotFound();

                    model = new AdminPropertyFormViewModel
                    {
                        Category = "Hotel",
                        Name = item.Name,
                        Location = item.City ?? "",
                        Address = item.Address,
                        ShortDescription = item.ShortDescription,
                        Description = item.Description,
                        ImageUrl = item.ImageUrl,
                        Price = item.StartingPricePerNight,
                        StarRating = item.StarRating,
                        ContactPhone = item.ContactPhone,
                        IsPublished = item.IsPublished,
                        IsFeatured = item.IsFeatured,
                        DisplayOrder = item.DisplayOrder,
                        DestinationId = item.DestinationId ?? 0
                    };
                    break;
                }

            case "Restaurant":
                {
                    var item = await _context.Restaurants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (item == null)
                        return NotFound();

                    model = new AdminPropertyFormViewModel
                    {
                        Category = "Restaurant",
                        Name = item.Name,
                        Location = item.City ?? "",
                        Address = item.Address,
                        ShortDescription = item.ShortDescription,
                        Description = item.Description,
                        ImageUrl = item.ImageUrl,
                        Price = item.AverageMealPrice,
                        CuisineType = item.CuisineType,
                        ContactPhone = item.ContactPhone,
                        IsPublished = item.IsPublished,
                        IsFeatured = item.IsFeatured,
                        DisplayOrder = item.DisplayOrder,
                        DestinationId = item.DestinationId ?? 0
                    };
                    break;
                }

            case "Resort":
                {
                    var item = await _context.Resorts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (item == null)
                        return NotFound();

                    model = new AdminPropertyFormViewModel
                    {
                        Category = "Resort",
                        Name = item.Name,
                        Location = item.CityOrRegion ?? "",
                        Address = item.Address,
                        ShortDescription = item.ShortDescription,
                        Description = item.Description,
                        ImageUrl = item.ImageUrl,
                        Price = item.StartingPricePerNight,
                        ContactPhone = item.ContactPhone,
                        IsPublished = item.IsPublished,
                        IsFeatured = item.IsFeatured,
                        DisplayOrder = item.DisplayOrder,
                        DestinationId = item.DestinationId ?? 0
                    };
                    break;
                }

            default:
                return BadRequest("Invalid property category.");
        }

        ViewData["Title"] = $"Edit {model.Category}";
        ViewData["Section"] = "Stays";
        ViewData["PropertyId"] = id;
        ViewBag.Destinations = await GetPropertyDestinationsAsync();

        return View("PropertyForm", model);
    }

    // =========================================================
    // PROPERTIES - UPDATE
    // =========================================================

    [HttpPost("Properties/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProperty(
        int id,
        AdminPropertyFormViewModel model)
    {
        ViewData["Title"] = "Edit Property";
        ViewData["Section"] = "Stays";
        ViewData["PropertyId"] = id;
        ViewBag.Destinations = await GetPropertyDestinationsAsync();

        if (!ValidatePropertyForm(model))
        {
            return View("PropertyForm", model);
        }

        if (!await ValidatePropertyDestinationAsync(model))
            return View("PropertyForm", model);

        var slug = CreatePropertySlug(model.Name);

        if (await PropertySlugExistsAsync(model.Category, slug, id))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Another property in this category already uses this name.");

            return View("PropertyForm", model);
        }

        switch (model.Category)
        {
            case "Hotel":
                {
                    var item = await _context.Hotels.FindAsync(id);
                    if (item == null)
                        return NotFound();

                    item.Name = model.Name.Trim();
                    item.Slug = slug;
                    item.City = model.Location.Trim();
                    item.Address = model.Address.Trim();
                    item.ShortDescription = model.ShortDescription.Trim();
                    item.Description = model.Description.Trim();
                    item.ImageUrl = model.ImageUrl.Trim();
                    item.StartingPricePerNight = model.Price;
                    item.StarRating = model.StarRating;
                    item.ContactPhone = model.ContactPhone.Trim();
                    item.IsPublished = model.IsPublished;
                    item.IsFeatured = model.IsFeatured;
                    item.DisplayOrder = model.DisplayOrder;
                    item.DestinationId = model.DestinationId;
                    break;
                }

            case "Restaurant":
                {
                    var item = await _context.Restaurants.FindAsync(id);
                    if (item == null)
                        return NotFound();

                    item.Name = model.Name.Trim();
                    item.Slug = slug;
                    item.City = model.Location.Trim();
                    item.Address = model.Address.Trim();
                    item.ShortDescription = model.ShortDescription.Trim();
                    item.Description = model.Description.Trim();
                    item.ImageUrl = model.ImageUrl.Trim();
                    item.AverageMealPrice = model.Price;
                    item.CuisineType = model.CuisineType!.Trim();
                    item.ContactPhone = model.ContactPhone.Trim();
                    item.IsPublished = model.IsPublished;
                    item.IsFeatured = model.IsFeatured;
                    item.DisplayOrder = model.DisplayOrder;
                    item.DestinationId = model.DestinationId;
                    break;
                }

            case "Resort":
                {
                    var item = await _context.Resorts.FindAsync(id);
                    if (item == null)
                        return NotFound();

                    item.Name = model.Name.Trim();
                    item.Slug = slug;
                    item.CityOrRegion = model.Location.Trim();
                    item.Address = model.Address.Trim();
                    item.ShortDescription = model.ShortDescription.Trim();
                    item.Description = model.Description.Trim();
                    item.ImageUrl = model.ImageUrl.Trim();
                    item.StartingPricePerNight = model.Price;
                    item.ContactPhone = model.ContactPhone.Trim();
                    item.IsPublished = model.IsPublished;
                    item.IsFeatured = model.IsFeatured;
                    item.DisplayOrder = model.DisplayOrder;
                    item.DestinationId = model.DestinationId;
                    break;
                }

            default:
                return BadRequest("Invalid property category.");
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.DestinationId), "The selected destination is no longer available. Select an existing destination and try again.");
            return View("PropertyForm", model);
        }

        TempData["SuccessMessage"] =
            $"{model.Category} updated successfully.";

        return RedirectToAction(nameof(Properties));
    }

    // =========================================================
    // PROPERTIES - DELETE
    // =========================================================

    [HttpPost("Properties/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProperty(
        int id,
        string category)
    {
        category = (category ?? string.Empty).Trim();

        switch (category)
        {
            case "Hotel":
                {
                    var item = await _context.Hotels.FindAsync(id);

                    if (item == null)
                    {
                        TempData["ErrorMessage"] = "Hotel not found.";
                        return RedirectToAction(nameof(Properties));
                    }

                    _context.Hotels.Remove(item);
                    break;
                }

            case "Restaurant":
                {
                    var item = await _context.Restaurants.FindAsync(id);

                    if (item == null)
                    {
                        TempData["ErrorMessage"] = "Restaurant not found.";
                        return RedirectToAction(nameof(Properties));
                    }

                    _context.Restaurants.Remove(item);
                    break;
                }

            case "Resort":
                {
                    var item = await _context.Resorts.FindAsync(id);

                    if (item == null)
                    {
                        TempData["ErrorMessage"] = "Resort not found.";
                        return RedirectToAction(nameof(Properties));
                    }

                    _context.Resorts.Remove(item);
                    break;
                }

            default:
                return BadRequest("Invalid property category.");
        }

        try
        {
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"{category} deleted successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "This property could not be deleted because it may be " +
                "referenced by existing bookings or other records.";
        }

        return RedirectToAction(nameof(Properties));
    }

    // =========================================================
    // PROPERTY FORM VALIDATION
    // =========================================================

    private bool ValidatePropertyForm(
        AdminPropertyFormViewModel model)
    {
        var allowedCategories = new[]
        {
            "Hotel", "Restaurant", "Resort"
        };

        if (!allowedCategories.Contains(model.Category))
        {
            ModelState.AddModelError(
                nameof(model.Category),
                "Select a valid property type.");

            return false;
        }

        if (model.Price is null or < 0)
        {
            ModelState.AddModelError(
                nameof(model.Price),
                "Enter a valid price.");
        }

        if (model.Category == "Hotel" &&
            model.StarRating is null or < 1 or > 5)
        {
            ModelState.AddModelError(
                nameof(model.StarRating),
                "Select a star rating from 1 to 5.");
        }

        if (model.Category == "Restaurant" &&
            string.IsNullOrWhiteSpace(model.CuisineType))
        {
            ModelState.AddModelError(
                nameof(model.CuisineType),
                "Enter the cuisine type.");
        }

        return ModelState.IsValid;
    }

    private async Task<bool> ValidatePropertyDestinationAsync(AdminPropertyFormViewModel model)
    {
        if (model.DestinationId <= 0 || !await _context.Destinations.AnyAsync(x => x.Id == model.DestinationId))
        {
            ModelState.AddModelError(nameof(model.DestinationId), "Select an existing destination or leave this field empty.");
            return false;
        }

        return true;
    }

    private Task<List<Destination>> GetPropertyDestinationsAsync() =>
        _context.Destinations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    // =========================================================
    // PROPERTY SLUG VALIDATION
    // =========================================================

    private async Task<bool> PropertySlugExistsAsync(
        string category,
        string slug,
        int? excludeId = null)
    {
        return category switch
        {
            "Hotel" => await _context.Hotels.AnyAsync(
                x => x.Slug == slug &&
                     (!excludeId.HasValue || x.Id != excludeId.Value)),

            "Restaurant" => await _context.Restaurants.AnyAsync(
                x => x.Slug == slug &&
                     (!excludeId.HasValue || x.Id != excludeId.Value)),

            "Resort" => await _context.Resorts.AnyAsync(
                x => x.Slug == slug &&
                     (!excludeId.HasValue || x.Id != excludeId.Value)),

            _ => true
        };
    }

    private static string CreatePropertySlug(string name)
    {
        var slug = (name ?? string.Empty).Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        return slug.Trim('-');
    }

    // =========================================================
    // TOURS - TOUR PACKAGE MANAGEMENT
    // =========================================================

    [HttpGet("Tours")]
    public async Task<IActionResult> Tours()
    {
        ViewData["Title"] = "Tour Packages";
        ViewData["Section"] = "Experiences";

        var packages = await _context.TourPackages
            .AsNoTracking()
            .Include(t => t.Destination)
            .Include(t => t.Departures)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .Select(t => new AdminTourPackageRowViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                DestinationName = t.Destination != null
                    ? t.Destination.Name
                    : t.DestinationName,
                ImageUrl = t.ImageUrl,
                DurationDays = t.DurationDays,
                PricePerAdult = t.PricePerAdult,
                PricePerChild = t.PricePerChild,
                IsPublished = t.IsPublished,
                IsFeatured = t.IsFeatured,
                DepartureCount = t.Departures.Count,
                DisplayOrder = t.DisplayOrder
            })
            .ToListAsync();

        return View("Tours", packages);
    }


    // ---------------------------------------------------------
    // CREATE TOUR - GET
    // ---------------------------------------------------------

    [HttpGet("Tours/Create")]
    public async Task<IActionResult> CreateTour()
    {
        ViewData["Title"] = "Create Tour Package";
        ViewData["Section"] = "Experiences";

        await PopulateTourDestinationsAsync();

        return View("TourEditor", new AdminTourPackageFormViewModel
        {
            DurationDays = 1
        });
    }


    // ---------------------------------------------------------
    // CREATE TOUR - POST
    // ---------------------------------------------------------

    [HttpPost("Tours/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTour(
        AdminTourPackageFormViewModel model)
    {
        ViewData["Title"] = "Create Tour Package";
        ViewData["Section"] = "Experiences";

        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Slug = string.IsNullOrWhiteSpace(model.Slug)
            ? CreateTourSlug(model.Name)
            : CreateTourSlug(model.Slug);

        if (await _context.TourPackages.AnyAsync(t => t.Slug == model.Slug))
        {
            ModelState.AddModelError(
                nameof(model.Slug),
                "This URL slug is already being used.");
        }

        var destination = await GetTourDestinationAsync(model.DestinationId);

        if (model.DestinationId.HasValue && destination == null)
        {
            ModelState.AddModelError(
                nameof(model.DestinationId),
                "Please select a valid destination.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateTourDestinationsAsync(model.DestinationId);
            return View("TourEditor", model);
        }

        var imageUrl = await SaveTourImageAsync(model.ImageFile);

        if (model.ImageFile != null && imageUrl == null)
        {
            ModelState.AddModelError(
                nameof(model.ImageFile),
                "The image could not be uploaded. Use a JPG, PNG or WebP image under 5 MB.");

            await PopulateTourDestinationsAsync(model.DestinationId);
            return View("TourEditor", model);
        }

        var package = new TourPackage
        {
            Name = model.Name,
            Slug = model.Slug,
            DestinationId = model.DestinationId,
            DestinationName = destination?.Name,
            ShortDescription = model.ShortDescription?.Trim(),
            Description = model.Description?.Trim(),
            DurationDays = model.DurationDays,
            PricePerAdult = model.PricePerAdult,
            PricePerChild = model.PricePerChild,
            Inclusions = model.Inclusions?.Trim(),
            Exclusions = model.Exclusions?.Trim(),
            ImageUrl = imageUrl ?? model.ImageUrl?.Trim(),
            IsPublished = model.IsPublished,
            IsFeatured = model.IsFeatured,
            DisplayOrder = model.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.TourPackages.Add(package);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tour package created successfully.";

        return RedirectToAction(nameof(Tours));
    }


    // ---------------------------------------------------------
    // EDIT TOUR - GET
    // ---------------------------------------------------------

    [HttpGet("Tours/Edit/{id:int}")]
    public async Task<IActionResult> EditTour(int id)
    {
        var package = await _context.TourPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (package == null)
            return NotFound();

        ViewData["Title"] = $"Edit {package.Name}";
        ViewData["Section"] = "Experiences";

        await PopulateTourDestinationsAsync(package.DestinationId);

        var model = new AdminTourPackageFormViewModel
        {
            Id = package.Id,
            Name = package.Name,
            Slug = package.Slug,
            DestinationId = package.DestinationId,
            ShortDescription = package.ShortDescription,
            Description = package.Description,
            DurationDays = package.DurationDays,
            PricePerAdult = package.PricePerAdult,
            PricePerChild = package.PricePerChild,
            Inclusions = package.Inclusions,
            Exclusions = package.Exclusions,
            ImageUrl = package.ImageUrl,
            IsPublished = package.IsPublished,
            IsFeatured = package.IsFeatured,
            DisplayOrder = package.DisplayOrder
        };

        return View("TourEditor", model);
    }


    // ---------------------------------------------------------
    // EDIT TOUR - POST
    // ---------------------------------------------------------

    [HttpPost("Tours/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTour(
        int id,
        AdminTourPackageFormViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var package = await _context.TourPackages
            .FirstOrDefaultAsync(t => t.Id == id);

        if (package == null)
            return NotFound();

        ViewData["Title"] = $"Edit {package.Name}";
        ViewData["Section"] = "Experiences";

        model.Name = model.Name?.Trim() ?? string.Empty;
        model.Slug = string.IsNullOrWhiteSpace(model.Slug)
            ? CreateTourSlug(model.Name)
            : CreateTourSlug(model.Slug);

        if (await _context.TourPackages.AnyAsync(
            t => t.Slug == model.Slug && t.Id != id))
        {
            ModelState.AddModelError(
                nameof(model.Slug),
                "This URL slug is already being used.");
        }

        var destination = await GetTourDestinationAsync(model.DestinationId);

        if (model.DestinationId.HasValue && destination == null)
        {
            ModelState.AddModelError(
                nameof(model.DestinationId),
                "Please select a valid destination.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateTourDestinationsAsync(model.DestinationId);
            return View("TourEditor", model);
        }

        string? newImageUrl = null;

        if (model.ImageFile != null)
        {
            newImageUrl = await SaveTourImageAsync(model.ImageFile);

            if (newImageUrl == null)
            {
                ModelState.AddModelError(
                    nameof(model.ImageFile),
                    "The image could not be uploaded. Use a JPG, PNG or WebP image under 5 MB.");

                await PopulateTourDestinationsAsync(model.DestinationId);
                return View("TourEditor", model);
            }
        }

        package.Name = model.Name;
        package.Slug = model.Slug;
        package.DestinationId = model.DestinationId;
        package.DestinationName = destination?.Name;
        package.ShortDescription = model.ShortDescription?.Trim();
        package.Description = model.Description?.Trim();
        package.DurationDays = model.DurationDays;
        package.PricePerAdult = model.PricePerAdult;
        package.PricePerChild = model.PricePerChild;
        package.Inclusions = model.Inclusions?.Trim();
        package.Exclusions = model.Exclusions?.Trim();
        package.IsPublished = model.IsPublished;
        package.IsFeatured = model.IsFeatured;
        package.DisplayOrder = model.DisplayOrder;
        package.UpdatedAt = DateTime.UtcNow;

        if (newImageUrl != null)
            package.ImageUrl = newImageUrl;
        else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
            package.ImageUrl = model.ImageUrl.Trim();

        await _context.SaveChangesAsync();

        TempData["Success"] = "Tour package updated successfully.";

        return RedirectToAction(nameof(Tours));
    }


    // ---------------------------------------------------------
    // DELETE TOUR - POST
    // ---------------------------------------------------------

    [HttpPost("Tours/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTour(int id)
    {
        var package = await _context.TourPackages
            .Include(t => t.Departures)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (package == null)
            return NotFound();

        var hasBookings = await _context.Bookings.AnyAsync(
            b => b.TourPackageId == id ||
                 (b.TourDeparture != null &&
                  b.TourDeparture.TourPackageId == id));

        if (hasBookings)
        {
            TempData["Error"] =
                "This package cannot be deleted because it has booking records. Unpublish it instead to preserve booking history.";

            return RedirectToAction(nameof(Tours));
        }

        var oldImageUrl = package.ImageUrl;

        _context.TourPackages.Remove(package);
        await _context.SaveChangesAsync();

        DeleteTourImage(oldImageUrl);

        TempData["Success"] = "Tour package deleted successfully.";

        return RedirectToAction(nameof(Tours));
    }

    // =========================================================
    // TOUR DEPARTURE MANAGEMENT
    // =========================================================

    [HttpGet("Tours/{packageId:int}/Departures")]
    public async Task<IActionResult> Departures(int packageId)
    {
        var package = await _context.TourPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == packageId);

        if (package == null)
            return NotFound();

        var departures = await _context.TourDepartures
            .AsNoTracking()
            .Where(d => d.TourPackageId == packageId)
            .OrderBy(d => d.DepartureDate)
            .ToListAsync();

        var departureIds = departures.Select(d => d.Id).ToList();

        var bookingRows = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.TourDepartureId.HasValue &&
                departureIds.Contains(b.TourDepartureId.Value) &&
                (b.Status == "Confirmed" || b.Status == "Completed"))
            .Select(b => new
            {
                DepartureId = b.TourDepartureId!.Value,
                b.Adults,
                b.Children,
                b.Status
            })
            .ToListAsync();

        var bookingSummary = bookingRows
            .GroupBy(b => b.DepartureId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    ReservedSeats = g.Sum(b => b.Adults + b.Children),
                    ConfirmedBookings = g.Count(b => b.Status == "Confirmed")
                });

        var model = departures.Select(d =>
        {
            bookingSummary.TryGetValue(d.Id, out var summary);

            return new AdminTourDepartureRowViewModel
            {
                Id = d.Id,
                TourPackageId = d.TourPackageId,
                PackageName = package.Name,
                DepartureDate = d.DepartureDate,
                ReturnDate = d.ReturnDate,
                MaximumGroupSize = d.MaximumGroupSize,
                ReservedSeats = summary?.ReservedSeats ?? 0,
                AvailableSeats = d.MaximumGroupSize -
                                 (summary?.ReservedSeats ?? 0),
                ConfirmedBookings = summary?.ConfirmedBookings ?? 0,
                IsActive = d.IsActive
            };
        }).ToList();

        ViewData["Title"] = "Tour Departures";
        ViewData["Section"] = "Experiences";
        ViewData["PackageName"] = package.Name;
        ViewData["PackageId"] = package.Id;

        return View(model);
    }


    // ---------------------------------------------------------
    // CREATE DEPARTURE
    // ---------------------------------------------------------

    [HttpGet("Tours/{packageId:int}/Departures/Create")]
    public async Task<IActionResult> CreateDeparture(int packageId)
    {
        var package = await _context.TourPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == packageId);

        if (package == null)
            return NotFound();

        ViewData["Title"] = "Create Tour Departure";
        ViewData["Section"] = "Experiences";

        return View("DepartureEditor", new AdminTourDepartureFormViewModel
        {
            TourPackageId = package.Id,
            PackageName = package.Name,
            DepartureDate = DateTime.Today.AddDays(7),
            ReturnDate = DateTime.Today.AddDays(12),
            MaximumGroupSize = 12,
            IsActive = true
        });
    }


    [HttpPost("Tours/{packageId:int}/Departures/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDeparture(
        int packageId,
        AdminTourDepartureFormViewModel model)
    {
        var package = await _context.TourPackages
            .FirstOrDefaultAsync(t => t.Id == packageId);

        if (package == null)
            return NotFound();

        model.TourPackageId = packageId;
        model.PackageName = package.Name;

        if (model.ReturnDate.Date < model.DepartureDate.Date)
        {
            ModelState.AddModelError(
                nameof(model.ReturnDate),
                "Return date cannot be before the departure date.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Tour Departure";
            ViewData["Section"] = "Experiences";
            return View("DepartureEditor", model);
        }

        var departure = new TourDeparture
        {
            TourPackageId = packageId,
            DepartureDate = model.DepartureDate.Date,
            ReturnDate = model.ReturnDate.Date,
            MaximumGroupSize = model.MaximumGroupSize,
            AvailableSeats = model.MaximumGroupSize,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TourDepartures.Add(departure);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tour departure created successfully.";

        return RedirectToAction(nameof(Departures), new { packageId });
    }


    // ---------------------------------------------------------
    // EDIT DEPARTURE
    // ---------------------------------------------------------

    [HttpGet("Tours/{packageId:int}/Departures/Edit/{id:int}")]
    public async Task<IActionResult> EditDeparture(int packageId, int id)
    {
        var departure = await _context.TourDepartures
            .Include(d => d.TourPackage)
            .FirstOrDefaultAsync(d =>
                d.Id == id && d.TourPackageId == packageId);

        if (departure == null)
            return NotFound();

        var model = new AdminTourDepartureFormViewModel
        {
            Id = departure.Id,
            TourPackageId = departure.TourPackageId,
            PackageName = departure.TourPackage.Name,
            DepartureDate = departure.DepartureDate,
            ReturnDate = departure.ReturnDate,
            MaximumGroupSize = departure.MaximumGroupSize,
            IsActive = departure.IsActive
        };

        ViewData["Title"] = "Edit Tour Departure";
        ViewData["Section"] = "Experiences";

        return View("DepartureEditor", model);
    }


    [HttpPost("Tours/{packageId:int}/Departures/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDeparture(
        int packageId,
        int id,
        AdminTourDepartureFormViewModel model)
    {
        var departure = await _context.TourDepartures
            .Include(d => d.TourPackage)
            .FirstOrDefaultAsync(d =>
                d.Id == id && d.TourPackageId == packageId);

        if (departure == null)
            return NotFound();

        model.Id = id;
        model.TourPackageId = packageId;
        model.PackageName = departure.TourPackage.Name;

        if (model.ReturnDate.Date < model.DepartureDate.Date)
        {
            ModelState.AddModelError(
                nameof(model.ReturnDate),
                "Return date cannot be before the departure date.");
        }

        var occupiedSeats = await _context.Bookings
            .Where(b =>
                b.TourDepartureId == id &&
                (b.Status == "Confirmed" || b.Status == "Completed"))
            .SumAsync(b => (int?)(b.Adults + b.Children)) ?? 0;

        if (model.MaximumGroupSize < occupiedSeats)
        {
            ModelState.AddModelError(
                nameof(model.MaximumGroupSize),
                $"Group size cannot be lower than the {occupiedSeats} seats already reserved.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Tour Departure";
            ViewData["Section"] = "Experiences";
            return View("DepartureEditor", model);
        }

        departure.DepartureDate = model.DepartureDate.Date;
        departure.ReturnDate = model.ReturnDate.Date;
        departure.MaximumGroupSize = model.MaximumGroupSize;
        departure.AvailableSeats = model.MaximumGroupSize - occupiedSeats;
        departure.IsActive = model.IsActive;
        departure.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Tour departure updated successfully.";

        return RedirectToAction(nameof(Departures), new { packageId });
    }


    // ---------------------------------------------------------
    // DELETE DEPARTURE
    // ---------------------------------------------------------

    [HttpPost("Tours/{packageId:int}/Departures/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDeparture(int packageId, int id)
    {
        var departure = await _context.TourDepartures
            .FirstOrDefaultAsync(d =>
                d.Id == id && d.TourPackageId == packageId);

        if (departure == null)
            return NotFound();

        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.TourDepartureId == id);

        if (hasBookings)
        {
            TempData["Error"] =
                "This departure cannot be deleted because booking records are linked to it.";

            return RedirectToAction(nameof(Departures), new { packageId });
        }

        _context.TourDepartures.Remove(departure);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Tour departure deleted successfully.";

        return RedirectToAction(nameof(Departures), new { packageId });
    }


    // =========================================================
    // TOUR PACKAGE HELPERS
    // =========================================================

    private async Task PopulateTourDestinationsAsync(int? selectedId = null)
    {
        var destinations = await _context.Destinations
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name,
                Selected = selectedId == d.Id
            })
            .ToListAsync();

        ViewBag.Destinations = destinations;
    }


    private async Task<Destination?> GetTourDestinationAsync(int? destinationId)
    {
        if (!destinationId.HasValue)
            return null;

        return await _context.Destinations
            .FirstOrDefaultAsync(d => d.Id == destinationId.Value);
    }


    private static string CreateTourSlug(string value)
    {
        value = value.Trim().ToLowerInvariant();

        value = Regex.Replace(value, @"[^a-z0-9]+", "-");

        return value.Trim('-');
    }


    private async Task<string?> SaveTourImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        if (file.Length > 5 * 1024 * 1024)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
            return null;

        var allowedContentTypes = new[]
        {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return null;

        var uploadDirectory = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "tours");

        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/tours/{fileName}";
    }


    private void DeleteTourImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith("/uploads/tours/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(imageUrl);

        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var filePath = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "tours",
            fileName);

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }
    // =========================================================
    // BOOKINGS
    // =========================================================

    [HttpGet("Bookings")]
    public async Task<IActionResult> Bookings(
        string? status,
        string? search)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            !status.Equals(
                "all",
                StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(b =>
                b.ReferenceNumber.Contains(term) ||
                b.CustomerName.Contains(term) ||
                b.Phone.Contains(term) ||
                b.ServiceName.Contains(term));
        }

        ViewBag.StatusFilter = status ?? "all";
        ViewBag.SearchTerm = search ?? "";

        ViewBag.TotalBookings =
            await _context.Bookings.CountAsync();

        ViewBag.PendingBookings =
            await _context.Bookings
                .CountAsync(b => b.Status == "Pending");

        ViewBag.ConfirmedBookings =
            await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed");

        ViewBag.CompletedBookings =
            await _context.Bookings
                .CountAsync(b => b.Status == "Completed");

        ViewBag.CancelledBookings =
            await _context.Bookings
                .CountAsync(b => b.Status == "Cancelled");

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }

    [HttpGet("Customers")]
    public async Task<IActionResult> Customers(CancellationToken cancellationToken)
    {
        var customers = await _context.CustomerAccounts
            .AsNoTracking()
            .Select(customer => new AdminCustomerViewModel
            {
                Name = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                AccountType = "Customer account",
                EmailConfirmed = customer.EmailConfirmed,
                CreatedAt = customer.CreatedAt,
                BookingCount = customer.Bookings.Count()
            })
            .ToListAsync(cancellationToken);

        var legacyCustomers = await _context.AdminUsers
            .AsNoTracking()
            .Where(user => user.Role != "Admin")
            .Select(user => new AdminCustomerViewModel
            {
                Name = user.DisplayName,
                Email = user.Email,
                AccountType = "Legacy customer account",
                EmailConfirmed = true,
                CreatedAt = DateTime.MinValue,
                BookingCount = _context.Bookings.Count(booking => booking.OwnerEmail == user.Email.ToLower())
            })
            .ToListAsync(cancellationToken);

        customers.AddRange(legacyCustomers);
        return View(customers.OrderBy(x => x.Name).ToList());
    }

    // =========================================================
    // BOOKING DETAILS
    // =========================================================

    [HttpGet("BookingDetails/{id:int}")]
    public async Task<IActionResult> BookingDetails(int id)
    {
        var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Tickets)
                .Include(b => b.StatusHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    // =========================================================
    // UPDATE BOOKING STATUS AND ADMIN NOTES
    // =========================================================

    [HttpPost("BookingDetails/{id:int}/Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBooking(
        int id,
        string status,
        string? adminNotes)
    {
        var allowedStatuses = new[]
        {
        "Pending",
        "Confirmed",
        "Rejected",
        "Completed",
        "Cancelled"
    };

        if (string.IsNullOrWhiteSpace(status) ||
            !allowedStatuses.Contains(status))
        {
            TempData["BookingError"] = "Invalid booking status.";

            return RedirectToAction(
                nameof(BookingDetails),
                new { id });
        }

        if (adminNotes?.Length > 1000)
        {
            TempData["BookingError"] =
                "Admin notes cannot exceed 1000 characters.";

            return RedirectToAction(
                nameof(BookingDetails),
                new { id });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (!Booking.CanTransition(booking.Status, status))
            {
                await transaction.RollbackAsync();
                TempData["BookingError"] = $"A booking cannot move from {booking.Status} to {status}.";
                return RedirectToAction(nameof(BookingDetails), new { id });
            }

            // Only tour bookings linked to a scheduled departure
            // participate in departure capacity management.
            if (booking.ServiceType == "Tour" &&
                booking.TourDepartureId.HasValue)
            {
                var departure = await _context.TourDepartures
                    .FirstOrDefaultAsync(d =>
                        d.Id == booking.TourDepartureId.Value);

                if (departure == null ||
                    !booking.TourPackageId.HasValue ||
                    departure.TourPackageId != booking.TourPackageId.Value)
                {
                    await transaction.RollbackAsync();

                    TempData["BookingError"] =
                        "The linked tour departure is invalid. " +
                        "Please review this booking.";

                    return RedirectToAction(
                        nameof(BookingDetails),
                        new { id });
                }

                bool targetOccupiesSeats =
                    status == "Confirmed" ||
                    status == "Completed";

                if (status == "Confirmed" && !departure.IsActive)
                {
                    await transaction.RollbackAsync();

                    TempData["BookingError"] =
                        "This departure is inactive and cannot receive " +
                        "new confirmations.";

                    return RedirectToAction(
                        nameof(BookingDetails),
                        new { id });
                }

                int requestedSeats =
                    booking.Adults + booking.Children;

                if (requestedSeats < 1)
                {
                    await transaction.RollbackAsync();

                    TempData["BookingError"] =
                        "This booking has an invalid passenger count.";

                    return RedirectToAction(
                        nameof(BookingDetails),
                        new { id });
                }

                // Count reservations belonging to OTHER bookings.
                int occupiedByOtherBookings =
                    await _context.Bookings
                        .Where(b =>
                            b.Id != booking.Id &&
                            b.TourDepartureId == departure.Id &&
                            (
                                b.Status == "Confirmed" ||
                                b.Status == "Completed"
                            ))
                        .SumAsync(b =>
                            (int?)(b.Adults + b.Children)) ?? 0;

                int newOccupiedSeats =
                    occupiedByOtherBookings +
                    (targetOccupiesSeats ? requestedSeats : 0);

                if (newOccupiedSeats > departure.MaximumGroupSize)
                {
                    await transaction.RollbackAsync();

                    int remainingSeats =
                        departure.MaximumGroupSize -
                        occupiedByOtherBookings;

                    TempData["BookingError"] =
                        $"Cannot confirm this booking. Only " +
                        $"{Math.Max(0, remainingSeats)} seats remain, " +
                        $"but this booking requires {requestedSeats}.";

                    return RedirectToAction(
                        nameof(BookingDetails),
                        new { id });
                }

                // Keep the stored availability synchronized.
                departure.AvailableSeats =
                    departure.MaximumGroupSize - newOccupiedSeats;

                departure.UpdatedAt = DateTime.UtcNow;
            }

            booking.AdminNotes = adminNotes?.Trim();
            var changedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _bookingStatusService.ChangeStatusAsync(
                    booking,
                    status,
                    changedByUserId,
                    "Admin",
                    adminNotes,
                    HttpContext.RequestAborted))
            {
                await transaction.RollbackAsync();
                TempData["BookingError"] = $"A booking cannot move from {booking.Status} to {status}.";
                return RedirectToAction(nameof(BookingDetails), new { id });
            }

            await _auditLogService.RecordAsync(
                User.Identity?.Name,
                "Booking status changed",
                $"Booking {booking.ReferenceNumber} changed to {booking.Status}.",
                "Booking",
                booking.Id.ToString());

            await transaction.CommitAsync();

            TempData["BookingSuccess"] =
                "Booking updated successfully.";

            return RedirectToAction(
                nameof(BookingDetails),
                new { id });
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["BookingError"] =
                "The booking could not be updated. Please try again.";

            return RedirectToAction(
                nameof(BookingDetails),
                new { id });
        }
    }
    // =========================================================
    // TRANSPORT - VEHICLE MANAGEMENT
    // =========================================================

    [HttpGet("Transport/Vehicles")]
    public async Task<IActionResult> Vehicles()
    {
        ViewData["Title"] = "Vehicle Management";
        ViewData["Section"] = "Transport";

        var vehicles = await _context.TransportServices
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return View(vehicles);
    }

    // CREATE VEHICLE FORM
    [HttpGet("Transport/Vehicles/Create")]
    public IActionResult CreateVehicle()
    {
        ViewData["Title"] = "Add Vehicle";
        ViewData["Section"] = "Transport";

        return View("VehicleForm", new TransportService
        {
            IsPublished = false,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    // CREATE VEHICLE
    [HttpPost("Transport/Vehicles/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVehicle(
        TransportService model,
        IFormFile? VehicleImage)
    {
        ViewData["Title"] = "Add Vehicle";
        ViewData["Section"] = "Transport";

        model.Name = (model.Name ?? "").Trim();
        model.Slug = (model.Slug ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Vehicle name is required.");

        if (string.IsNullOrWhiteSpace(model.Slug))
            ModelState.AddModelError(nameof(model.Slug), "URL slug is required.");

        if (model.PassengerCapacity.HasValue && model.PassengerCapacity <= 0)
            ModelState.AddModelError(nameof(model.PassengerCapacity), "Passenger capacity must be greater than zero.");

        if (model.StartingPrice.HasValue && model.StartingPrice < 0)
            ModelState.AddModelError(nameof(model.StartingPrice), "Starting price cannot be negative.");

        if (!string.IsNullOrWhiteSpace(model.Slug) &&
            await _context.TransportServices.AnyAsync(x => x.Slug == model.Slug))
        {
            ModelState.AddModelError(nameof(model.Slug), "This URL slug is already in use.");
        }

        if (!ModelState.IsValid)
            return View("VehicleForm", model);

        if (VehicleImage != null && VehicleImage.Length > 0)
        {
            try
            {
                model.ImageUrl = await SaveTransportImageAsync(VehicleImage);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(VehicleImage), ex.Message);
                return View("VehicleForm", model);
            }
            catch (IOException)
            {
                ModelState.AddModelError(nameof(VehicleImage), "The image could not be saved.");
                return View("VehicleForm", model);
            }
        }

        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = null;

        _context.TransportServices.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle added successfully.";

        return RedirectToAction(nameof(Vehicles));
    }

    // EDIT VEHICLE FORM
    [HttpGet("Transport/Vehicles/Edit/{id:int}")]
    public async Task<IActionResult> EditVehicle(int id)
    {
        var vehicle = await _context.TransportServices.FindAsync(id);

        if (vehicle == null)
            return NotFound();

        ViewData["Title"] = "Edit Vehicle";
        ViewData["Section"] = "Transport";

        return View("VehicleForm", vehicle);
    }

    // UPDATE VEHICLE
    [HttpPost("Transport/Vehicles/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVehicle(
        int id,
        TransportService model,
        IFormFile? VehicleImage)
    {
        var vehicle = await _context.TransportServices.FindAsync(id);

        if (vehicle == null)
            return NotFound();

        ViewData["Title"] = "Edit Vehicle";
        ViewData["Section"] = "Transport";

        model.Name = (model.Name ?? "").Trim();
        model.Slug = (model.Slug ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Vehicle name is required.");

        if (string.IsNullOrWhiteSpace(model.Slug))
            ModelState.AddModelError(nameof(model.Slug), "URL slug is required.");

        if (model.PassengerCapacity.HasValue && model.PassengerCapacity <= 0)
            ModelState.AddModelError(nameof(model.PassengerCapacity), "Passenger capacity must be greater than zero.");

        if (model.StartingPrice.HasValue && model.StartingPrice < 0)
            ModelState.AddModelError(nameof(model.StartingPrice), "Starting price cannot be negative.");

        if (!string.IsNullOrWhiteSpace(model.Slug) &&
            await _context.TransportServices.AnyAsync(
                x => x.Slug == model.Slug && x.Id != id))
        {
            ModelState.AddModelError(nameof(model.Slug), "This URL slug is already in use.");
        }

        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.ImageUrl = vehicle.ImageUrl;
            return View("VehicleForm", model);
        }

        string? newImagePath = null;

        if (VehicleImage != null && VehicleImage.Length > 0)
        {
            try
            {
                newImagePath = await SaveTransportImageAsync(VehicleImage);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(VehicleImage), ex.Message);
                model.Id = id;
                model.ImageUrl = vehicle.ImageUrl;
                return View("VehicleForm", model);
            }
            catch (IOException)
            {
                ModelState.AddModelError(nameof(VehicleImage), "The image could not be saved.");
                model.Id = id;
                model.ImageUrl = vehicle.ImageUrl;
                return View("VehicleForm", model);
            }
        }

        // Update vehicle information.
        // DestinationId is preserved because it is not included in this form.
        vehicle.Name = model.Name;
        vehicle.Slug = model.Slug;
        vehicle.VehicleType = model.VehicleType;
        vehicle.StartingCity = model.StartingCity;
        vehicle.DestinationCity = model.DestinationCity;
        vehicle.Description = model.Description;
        vehicle.PassengerCapacity = model.PassengerCapacity;
        vehicle.StartingPrice = model.StartingPrice;
        vehicle.ContactPhone = model.ContactPhone;
        vehicle.IsPublished = model.IsPublished;
        vehicle.IsFeatured = model.IsFeatured;
        vehicle.DisplayOrder = model.DisplayOrder;
        vehicle.UpdatedAt = DateTime.UtcNow;

        if (newImagePath != null)
        {
            var oldImagePath = vehicle.ImageUrl;
            vehicle.ImageUrl = newImagePath;

            DeleteTransportImage(oldImagePath);
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle updated successfully.";

        return RedirectToAction(nameof(Vehicles));
    }

    [HttpPost("Transport/Vehicles/Toggle/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVehicle(int id)
    {
        var vehicle = await _context.TransportServices.FindAsync(id);

        if (vehicle == null)
            return NotFound();

        vehicle.IsPublished = !vehicle.IsPublished;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = vehicle.IsPublished
            ? "Vehicle published successfully."
            : "Vehicle unpublished successfully.";

        return RedirectToAction(nameof(Vehicles));
    }

    [HttpPost("Transport/Vehicles/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var vehicle = await _context.TransportServices.FindAsync(id);

        if (vehicle == null)
            return NotFound();

        var hasRoutes = await _context.TransportRoutePricings
            .AnyAsync(route => route.TransportServiceId == id);
        var hasBookings = await _context.Bookings
            .AnyAsync(booking => booking.TransportServiceId == id);

        if (hasRoutes || hasBookings)
        {
            TempData["ErrorMessage"] =
                "This vehicle cannot be deleted because it has routes or booking history. Unpublish it instead.";
            return RedirectToAction(nameof(Vehicles));
        }

        DeleteTransportImage(vehicle.ImageUrl);
        _context.TransportServices.Remove(vehicle);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle deleted successfully.";
        return RedirectToAction(nameof(Vehicles));
    }

    // =========================================================
    // TRANSPORT - IMAGE UPLOAD
    // =========================================================

    private async Task<string?> SaveTransportImageAsync(IFormFile imageFile)
    {
        const long maxFileSize = 5 * 1024 * 1024;

        if (imageFile.Length <= 0 || imageFile.Length > maxFileSize)
            throw new InvalidOperationException("Image size must not exceed 5 MB.");

        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
            throw new IOException("The wwwroot folder could not be located.");

        var header = new byte[12];
        int bytesRead;

        await using (var input = imageFile.OpenReadStream())
        {
            bytesRead = await input.ReadAsync(header, 0, header.Length);
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
            extension = ".jpg";
        else if (isPng)
            extension = ".png";
        else if (isWebp)
            extension = ".webp";
        else
            throw new InvalidOperationException("Choose a valid JPG, PNG or WebP image.");

        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "images",
            "transport");

        Directory.CreateDirectory(uploadsFolder);

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

        return $"/images/transport/{fileName}";
    }

    // Delete only images managed by this transport upload system.
    private void DeleteTransportImage(string? imagePath)
    {
        const string prefix = "/images/transport/";

        if (string.IsNullOrWhiteSpace(imagePath) ||
            !imagePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        var fileName = Path.GetFileName(imagePath);

        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var fullPath = Path.Combine(
            _environment.WebRootPath,
            "images",
            "transport",
            fileName);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }


    // =========================================================
    // TRANSPORT - ROUTE PRICING MANAGEMENT
    // =========================================================

    [HttpGet("Transport")]
    public async Task<IActionResult> Transport(int? editId)
    {
        ViewData["Title"] = "Transport & Route Pricing";
        ViewData["Section"] = "Transport";

        ViewBag.TransportServices = await _context.TransportServices
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        if (editId.HasValue)
        {
            var editRoute = await _context.TransportRoutePricings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value);

            if (editRoute == null)
            {
                return NotFound();
            }

            ViewBag.EditRoute = editRoute;
        }

        var routes = await _context.TransportRoutePricings
            .AsNoTracking()
            .Include(x => x.TransportService)
            .OrderBy(x => x.TransportService!.Name)
            .ThenBy(x => x.PickupCity)
            .ToListAsync();

        return View(routes);
    }

    // =========================================================
    // TRANSPORT - CREATE / UPDATE ROUTE
    // =========================================================

    [HttpPost("Transport/SaveRoute")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTransportRoute(
        TransportRoutePricing model)
    {
        model.PickupCity = (model.PickupCity ?? "").Trim();
        model.DropoffCity = (model.DropoffCity ?? "").Trim();

        if (string.IsNullOrWhiteSpace(model.PickupCity))
        {
            ModelState.AddModelError(
                nameof(model.PickupCity),
                "Pickup city is required.");
        }

        if (string.IsNullOrWhiteSpace(model.DropoffCity))
        {
            ModelState.AddModelError(
                nameof(model.DropoffCity),
                "Drop-off city is required.");
        }

        if (model.PickupCity.Equals(
            model.DropoffCity,
            StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.DropoffCity),
                "Pickup and drop-off cities must be different.");
        }

        if (model.OneWayFarePKR <= 0)
        {
            ModelState.AddModelError(
                nameof(model.OneWayFarePKR),
                "Enter a one-way fare greater than zero.");
        }

        // Return fare is optional.
        // If empty, the booking system uses the one-way fare.
        if (model.ReturnFarePKR.HasValue &&
            model.ReturnFarePKR.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(model.ReturnFarePKR),
                "Return fare must be greater than zero, or leave it empty.");
        }

        var vehicleExists = await _context.TransportServices
            .AnyAsync(x => x.Id == model.TransportServiceId);

        if (!vehicleExists)
        {
            ModelState.AddModelError(
                nameof(model.TransportServiceId),
                "Select a valid vehicle.");
        }

        var duplicate = await _context.TransportRoutePricings
            .AnyAsync(x =>
                x.TransportServiceId == model.TransportServiceId &&
                x.PickupCity == model.PickupCity &&
                x.DropoffCity == model.DropoffCity &&
                x.Id != model.Id);

        if (duplicate)
        {
            ModelState.AddModelError(
                nameof(model.DropoffCity),
                "This route already exists for the selected vehicle.");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] =
                string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

            return RedirectToAction(nameof(Transport));
        }

        // Remember whether this is a new record before EF assigns its ID.
        bool isNewRoute = model.Id == 0;

        if (isNewRoute)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = null;

            _context.TransportRoutePricings.Add(model);
        }
        else
        {
            var existing = await _context.TransportRoutePricings
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.TransportServiceId = model.TransportServiceId;
            existing.PickupCity = model.PickupCity;
            existing.DropoffCity = model.DropoffCity;

            // Save both fares.
            existing.OneWayFarePKR = model.OneWayFarePKR;
            existing.ReturnFarePKR = model.ReturnFarePKR;

            existing.IsActive = model.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isNewRoute
                ? "Transport route added successfully."
                : "Transport route updated successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] =
                "The route could not be saved. Check whether this vehicle and route already exist, and verify that the database has the ReturnFarePKR column.";
        }

        return RedirectToAction(nameof(Transport));
    }

    // =========================================================
    // TRANSPORT - ACTIVATE / DEACTIVATE ROUTE
    // =========================================================

    [HttpPost("Transport/Toggle/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTransportRoute(int id)
    {
        var route = await _context.TransportRoutePricings
            .FirstOrDefaultAsync(x => x.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        route.IsActive = !route.IsActive;
        route.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = route.IsActive
            ? "Route activated successfully."
            : "Route deactivated successfully.";

        return RedirectToAction(nameof(Transport));
    }


    // =========================================================
    // OTHER ADMIN PAGES
    // =========================================================

    [HttpGet("Calendar")]
    public IActionResult Calendar()
    {
        ViewData["Title"] = "Calendar";
        ViewData["Section"] = "Calendar";

        return View("ComingSoon");
    }

    [HttpGet("Inquiries")]
    public async Task<IActionResult> Inquiries()
    {
        ViewData["Title"] = "Inquiries";
        ViewData["Section"] = "Inquiries";

        var inquiries = await _context.ContactInquiries
            .AsNoTracking()
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync();

        return View(inquiries);
    }

    [HttpGet("InquiryDetails/{id:int}")]
    public async Task<IActionResult> InquiryDetails(int id)
    {
        var inquiry = await _context.ContactInquiries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inquiry == null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Inquiry Details";
        ViewData["Section"] = "Inquiries";

        return View(inquiry);
    }

    [HttpGet("CustomTripRequests")]
    public async Task<IActionResult> CustomTripRequests(string? status, string? search)
    {
        var query = _context.TripInquiries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ReferenceNumber.Contains(term) || x.CustomerName.Contains(term) || x.Destinations.Contains(term));
        }

        ViewData["Title"] = "Custom Trip Requests";
        ViewData["Section"] = "Inquiries";
        return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpGet("CustomTripRequests/{id:int}")]
    public async Task<IActionResult> CustomTripRequestDetails(int id)
    {
        var inquiry = await _context.TripInquiries.FirstOrDefaultAsync(x => x.Id == id);
        if (inquiry == null)
            return NotFound();

        ViewData["Title"] = "Custom Trip Request";
        ViewData["Section"] = "Inquiries";
        return View(inquiry);
    }

    [HttpPost("CustomTripRequests/{id:int}/Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCustomTripRequest(int id, string status, string? adminNotes, string? proposedOffer)
    {
        var allowedStatuses = new[] { "Pending", "In Progress", "Offer Prepared", "Accepted", "Declined", "Cancelled" };
        if (!allowedStatuses.Contains(status) || adminNotes?.Length > 2000 || proposedOffer?.Length > 2000)
        {
            TempData["InquiryError"] = "Invalid custom request update.";
            return RedirectToAction(nameof(CustomTripRequestDetails), new { id });
        }

        var inquiry = await _context.TripInquiries.FirstOrDefaultAsync(x => x.Id == id);
        if (inquiry == null)
            return NotFound();

        inquiry.Status = status;
        inquiry.AdminNotes = string.IsNullOrWhiteSpace(adminNotes) ? null : adminNotes.Trim();
        inquiry.ProposedOffer = string.IsNullOrWhiteSpace(proposedOffer) ? null : proposedOffer.Trim();
        await _context.SaveChangesAsync();
        TempData["InquirySuccess"] = "Custom trip request updated.";
        return RedirectToAction(nameof(CustomTripRequestDetails), new { id });
    }

    // =========================================================
    // REVIEWS - MODERATION DASHBOARD
    // =========================================================

    [HttpGet("Reviews")]
    public async Task<IActionResult> Reviews(
        string? status,
        string? search)
    {
        var query = _context.Reviews
            .AsNoTracking()
            .Include(r => r.Destination)
            .Include(r => r.Hotel)
            .Include(r => r.Restaurant)
            .Include(r => r.Resort)
            .Include(r => r.TransportService)
            .Include(r => r.TourPackage)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ReviewStatus>(
                status, true, out var selectedStatus))
        {
            query = query.Where(r => r.Status == selectedStatus);
        }
        else
        {
            status = "All";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(r =>
                r.ReviewerName.Contains(term) ||
                r.Title.Contains(term) ||
                r.Comment.Contains(term));
        }

        ViewData["Title"] = "Review Moderation";
        ViewData["Section"] = "Reviews";

        ViewBag.StatusFilter = status ?? "All";
        ViewBag.SearchTerm = search ?? "";

        ViewBag.TotalReviews = await _context.Reviews.CountAsync();

        ViewBag.PendingReviews = await _context.Reviews
            .CountAsync(r => r.Status == ReviewStatus.Pending);

        ViewBag.ApprovedReviews = await _context.Reviews
            .CountAsync(r => r.Status == ReviewStatus.Approved);

        ViewBag.RejectedReviews = await _context.Reviews
            .CountAsync(r => r.Status == ReviewStatus.Rejected);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }

    // =========================================================
    // REVIEWS - CHANGE MODERATION STATUS
    // =========================================================

    [HttpPost("Reviews/{id:int}/SetStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetReviewStatus(
        int id,
        string status,
        string? returnStatus,
        string? search)
    {
        if (!Enum.TryParse<ReviewStatus>(
                status, true, out var newStatus) ||
            !Enum.IsDefined(typeof(ReviewStatus), newStatus))
        {
            TempData["ErrorMessage"] = "Invalid review status.";

            return RedirectToAction(
                nameof(Reviews),
                new { status = returnStatus, search });
        }

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            TempData["ErrorMessage"] = "Review not found.";

            return RedirectToAction(
                nameof(Reviews),
                new { status = returnStatus, search });
        }

        review.Status = newStatus;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = newStatus switch
        {
            ReviewStatus.Approved =>
                "Review approved. It can now appear publicly.",

            ReviewStatus.Rejected =>
                "Review rejected. It will not appear publicly.",

            ReviewStatus.Pending =>
                "Review returned to pending moderation.",

            _ => "Review status updated."
        };

        return RedirectToAction(
            nameof(Reviews),
            new { status = returnStatus, search });
    }




    // ============================================================
    // WEBSITE CONTENT MANAGEMENT
    // ============================================================

    [HttpGet("WebsiteContent")]
    public async Task<IActionResult> WebsiteContent()
    {
        ViewData["Title"] = "Website content";
        ViewData["Section"] = "Website content";

        var content = await _websiteContentService.GetAsync();

        return View(content);
    }

    [HttpPost("WebsiteContent/Preview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewWebsiteContent(
        WebsiteContent model,
        string previewPage = "landing")
    {
        ViewData["Title"] = "Website content preview";
        ViewData["Section"] = "Website content";

        model.LogoUrl = await PreparePreviewImageAsync(
            model.LogoImageFile,
            model.LogoUrl,
            nameof(model.LogoImageFile));

        model.HomeHeroImageUrl = await PreparePreviewImageAsync(
            model.HomeHeroImageFile,
            model.HomeHeroImageUrl,
            nameof(model.HomeHeroImageFile));

        if (!ModelState.IsValid)
        {
            return View("WebsiteContent", model);
        }

        ViewBag.WebsiteContent = model;

        if (string.Equals(previewPage, "home", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.FeaturedDestinations = await _context.Destinations
                .Where(d => d.IsPublished && d.IsFeatured)
                .OrderBy(d => d.Name)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            return View("~/Views/Home/Index.cshtml");
        }

        return View("~/Views/Landing/Index.cshtml");
    }


    // ============================================================
    // WEBSITE CONTENT POST
    // ============================================================

    [HttpPost("WebsiteContent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WebsiteContent(WebsiteContent model)
    {
        ViewData["Title"] = "Website content";
        ViewData["Section"] = "Website content";

        // --------------------------------------------------------
        // VALIDATE LOGO IMAGE
        // --------------------------------------------------------

        if (model.LogoImageFile != null &&
            model.LogoImageFile.Length > 0)
        {
            var error = await GetWebsiteImageErrorAsync(model.LogoImageFile);

            if (error != null)
            {
                ModelState.AddModelError(
                    nameof(model.LogoImageFile),
                    error
                );
            }

        }

        // --------------------------------------------------------
        // VALIDATE HERO IMAGE
        // --------------------------------------------------------

        if (model.HomeHeroImageFile != null &&
            model.HomeHeroImageFile.Length > 0)
        {
            var error = await GetWebsiteImageErrorAsync(model.HomeHeroImageFile);

            if (error != null)
            {
                ModelState.AddModelError(
                    nameof(model.HomeHeroImageFile),
                    error
                );
            }
        }

        // --------------------------------------------------------
        // MODEL VALIDATION
        // --------------------------------------------------------

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // --------------------------------------------------------
        // LOAD EXISTING RECORD
        // --------------------------------------------------------

        var content = await _context.WebsiteContents
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (content == null)
        {
            content = new WebsiteContent();
            _context.WebsiteContents.Add(content);
        }

        // --------------------------------------------------------
        // BRAND IDENTITY
        // --------------------------------------------------------

        content.SiteName = model.SiteName ?? "Karnel Travel Guide";
        content.Tagline = model.Tagline ?? "";

        // Upload takes priority.
        // Otherwise, retain the existing image if the URL is empty.

        if (model.LogoImageFile != null &&
            model.LogoImageFile.Length > 0)
        {
            content.LogoUrl =
                await SaveWebsiteImageAsync(model.LogoImageFile);
        }
        else if (!string.IsNullOrWhiteSpace(model.LogoUrl))
        {
            content.LogoUrl = model.LogoUrl;
        }
        else
        {
            content.LogoUrl ??= "";
        }

        // --------------------------------------------------------
        // HOMEPAGE
        // --------------------------------------------------------

        content.HomeHeroHeading = model.HomeHeroHeading ?? "";
        content.HomeHeroSubtitle = model.HomeHeroSubtitle ?? "";
        content.HomeHeroButtonText = model.HomeHeroButtonText ?? "";

        if (model.HomeHeroImageFile != null &&
            model.HomeHeroImageFile.Length > 0)
        {
            content.HomeHeroImageUrl =
                await SaveWebsiteImageAsync(model.HomeHeroImageFile);
        }
        else if (!string.IsNullOrWhiteSpace(model.HomeHeroImageUrl))
        {
            content.HomeHeroImageUrl = model.HomeHeroImageUrl;
        }
        else
        {
            content.HomeHeroImageUrl ??= "";
        }

        // --------------------------------------------------------
        // ABOUT PAGE
        // --------------------------------------------------------

        content.AboutHeading = model.AboutHeading ?? "";
        content.AboutDescription = model.AboutDescription ?? "";
        content.Mission = model.Mission ?? "";
        content.Vision = model.Vision ?? "";

        // --------------------------------------------------------
        // CONTACT PAGE
        // --------------------------------------------------------

        content.ContactAddress = model.ContactAddress ?? "";
        content.ContactPhone = model.ContactPhone ?? "";
        content.ContactEmail = model.ContactEmail ?? "";

        // --------------------------------------------------------
        // FOOTER
        // --------------------------------------------------------

        content.FooterDescription = model.FooterDescription ?? "";
        content.FacebookUrl = model.FacebookUrl ?? "";
        content.InstagramUrl = model.InstagramUrl ?? "";
        content.YouTubeUrl = model.YouTubeUrl ?? "";

        // --------------------------------------------------------
        // SEO
        // --------------------------------------------------------

        content.DefaultMetaTitle = model.DefaultMetaTitle ?? "";
        content.DefaultMetaDescription = model.DefaultMetaDescription ?? "";

        // --------------------------------------------------------
        // UPDATE INFORMATION
        // --------------------------------------------------------

        content.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Website content and images have been saved successfully.";

        return RedirectToAction(nameof(WebsiteContent));
    }

    private async Task<string?> PreparePreviewImageAsync(
        IFormFile? file,
        string? existingUrl,
        string fieldName)
    {
        if (file == null || file.Length == 0)
        {
            return existingUrl;
        }

        var error = await GetWebsiteImageErrorAsync(file);
        if (error != null)
        {
            ModelState.AddModelError(fieldName, error);
            return existingUrl;
        }

        await using var input = file.OpenReadStream();
        await using var output = new MemoryStream();
        await input.CopyToAsync(output);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return $"data:{contentType};base64,{Convert.ToBase64String(output.ToArray())}";
    }


    // ============================================================
    // WEBSITE IMAGE VALIDATION
    // ============================================================

    private async Task<string?> GetWebsiteImageErrorAsync(IFormFile file)
    {
        const long maxFileSize = 5 * 1024 * 1024;

        if (file.Length <= 0)
            return "Please select a valid image.";

        if (file.Length > maxFileSize)
            return "Image size cannot exceed 5 MB.";

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var allowedExtensions = new[]
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

        if (!allowedExtensions.Contains(extension))
        {
            return "Only JPG, JPEG, PNG and WEBP images are allowed.";
        }

        // Verify actual file signature.

        await using var stream = file.OpenReadStream();

        var header = new byte[12];

        var bytesRead = await stream.ReadAsync(
            header,
            0,
            header.Length
        );

        bool validImage = extension switch
        {
            ".jpg" or ".jpeg" =>
                bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A,

            ".webp" =>
                bytesRead >= 12 &&
                header[0] == (byte)'R' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F' &&
                header[3] == (byte)'F' &&
                header[8] == (byte)'W' &&
                header[9] == (byte)'E' &&
                header[10] == (byte)'B' &&
                header[11] == (byte)'P',

            _ => false
        };

        return validImage
            ? null
            : "The selected file does not contain a valid image signature.";
    }


    // ============================================================
    // SAVE WEBSITE IMAGE
    // ============================================================

    private async Task<string> SaveWebsiteImageAsync(IFormFile file)
    {
        var webRoot = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadFolder = Path.Combine(
            webRoot,
            "uploads",
            "website"
        );

        Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var fileName = $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            uploadFolder,
            fileName
        );

        await using (var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/website/{fileName}";
    }


    // =========================================================
    // REPORTS - GENERATE REPORTS
    // =========================================================

    [HttpGet("Reports")]
    public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
    {
        ViewData["Title"] = "Reports & Analytics";
        ViewData["Section"] = "Reports";

        var query = _context.Bookings.AsNoTracking();

        // Filter by booking submission date.
        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value.Date);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedAt < endExclusive);
        }

        // Retrieve only the fields needed for reporting.
        var bookings = await query
            .Select(x => new
            {
                x.CreatedAt,
                x.Status,
                x.ServiceType,
                x.EstimatedTotalPKR
            })
            .ToListAsync();

        var model = new ReportsViewModel
        {
            FromDate = from,
            ToDate = to,

            TotalBookings = bookings.Count,

            PendingBookings = bookings.Count(x =>
                string.Equals(
                    x.Status,
                    "Pending",
                    StringComparison.OrdinalIgnoreCase)),

            ConfirmedBookings = bookings.Count(x =>
                string.Equals(
                    x.Status,
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase)),

            CompletedBookings = bookings.Count(x =>
                string.Equals(
                    x.Status,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase)),

            CancelledBookings = bookings.Count(x =>
                string.Equals(
                    x.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase)),

            // Only bookings with calculated estimates contribute.
            EstimatedBookingValue = bookings
                .Where(x => x.EstimatedTotalPKR.HasValue)
                .Sum(x => x.EstimatedTotalPKR ?? 0m),

            // Historical bookings without calculated amounts.
            UncalculatedBookings = bookings.Count(x =>
                !x.EstimatedTotalPKR.HasValue),

            // Monthly booking trends.
            MonthlyBookings = bookings
                .GroupBy(x => new
                {
                    x.CreatedAt.Year,
                    x.CreatedAt.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x => new MonthlyBookingReport
                {
                    Month = new DateTime(
                        x.Key.Year,
                        x.Key.Month,
                        1
                    ).ToString("MMM yyyy"),

                    BookingCount = x.Count()
                })
                .ToList(),

            // Booking demand and estimated value by service.
            ServicePerformance = bookings
                .GroupBy(x =>
                    string.IsNullOrWhiteSpace(x.ServiceType)
                        ? "Other"
                        : x.ServiceType.Trim())
                .OrderByDescending(x => x.Count())
                .Select(x => new ServiceBookingReport
                {
                    ServiceType = x.Key,
                    BookingCount = x.Count(),

                    EstimatedValue = x
                        .Where(b => b.EstimatedTotalPKR.HasValue)
                        .Sum(b => b.EstimatedTotalPKR ?? 0m)
                })
                .ToList()
        };

        return View(model);
    }


    // ==========================================
    // EXPORT REPORTS TO EXCEL-COMPATIBLE CSV
    // ==========================================

    [HttpGet("Reports/ExportCsv")]
    public async Task<IActionResult> ExportReportsCsv(
        DateTime? from,
        DateTime? to)
    {
        var query = _context.Bookings.AsNoTracking();

        // Apply the same date filters as the Reports page.
        if (from.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAt >= from.Value.Date);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.CreatedAt < endExclusive);
        }

        // Retrieve booking records for export.
        var bookings = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.ReferenceNumber,
                x.CustomerName,
                x.Phone,
                x.Email,
                x.ServiceType,
                x.ServiceName,
                x.BookingDate,
                x.CreatedAt,
                x.Adults,
                x.Children,
                x.Rooms,
                x.Status,
                x.EstimatedTotalPKR
            })
            .ToListAsync();

        // Escape values correctly for CSV.
        static string Csv(string? value)
        {
            value ??= "";

            return "\"" +
                value.Replace("\"", "\"\"") +
                "\"";
        }

        var csv = new StringBuilder();

        // Column headings.
        csv.AppendLine(
            "Reference Number,Customer Name,Phone,Email,Service Type,Service Name,Booking Date,Submitted At,Adults,Children,Rooms,Status,Estimated Amount (PKR)");

        // Booking rows.
        foreach (var booking in bookings)
        {
            csv.AppendLine(string.Join(",",
                Csv(booking.ReferenceNumber),
                Csv(booking.CustomerName),
                Csv(booking.Phone),
                Csv(booking.Email),
                Csv(booking.ServiceType),
                Csv(booking.ServiceName),
                Csv(booking.BookingDate.ToString("yyyy-MM-dd")),
                Csv(booking.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
                booking.Adults,
                booking.Children,
                booking.Rooms,
                Csv(booking.Status),
                booking.EstimatedTotalPKR?.ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) ?? ""
            ));
        }

        // UTF-8 BOM for better Excel compatibility.
        var encoding = new System.Text.UTF8Encoding(true);

        var fileBytes = encoding.GetPreamble()
            .Concat(encoding.GetBytes(csv.ToString()))
            .ToArray();

        var filename =
            $"KarnelTravel_Bookings_{DateTime.Now:yyyyMMdd_HHmm}.csv";

        return File(
            fileBytes,
            "text/csv; charset=utf-8",
            filename);
    }

    // =========================================================
    // SETTING- CHANGE MODERATION STATUS
    // =========================================================

    [HttpGet("Settings")]
    public async Task<IActionResult> Settings(string? search = null, string? action = null)
    {
        ViewData["Title"] = "Settings";
        ViewData["Section"] = "Settings";
        return View("Settings", new SettingsViewModel
        {
            Settings = await _settingsService.GetAsync(),
            AuditLogs = await _auditLogService.RecentAsync(search, action),
            EmailDeliveryConfigured = false,
            Search = search,
            ActionFilter = action
        });
    }

    [HttpPost("Settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AuditLogs = await _auditLogService.RecentAsync();
            model.EmailDeliveryConfigured = false;
            return View("Settings", model);
        }

        var administrator = User.Identity?.Name;
        await _settingsService.SaveAsync(model.Settings, administrator);
        await _auditLogService.RecordAsync(administrator, "Settings updated", "Application, website, booking, notification, and appearance settings were updated.", "ApplicationSettings", "1");
        TempData["SettingsSuccess"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpGet("Settings/Account")]
    public async Task<IActionResult> AccountSettings([FromServices] IAdminAuthService authService)
    {
        var user = await authService.FindByIdAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
        if (user is null) return Challenge();
        return View("AccountSettings", new AccountSettingsViewModel { DisplayName = user.DisplayName, Email = user.Email, Role = user.Role });
    }

    [HttpPost("Settings/Account")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AccountSettings(AccountSettingsViewModel model, [FromServices] IAdminAuthService authService)
    {
        var user = await authService.FindByIdAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
        if (user is null) return Challenge();
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) || !await authService.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword))
                ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is incorrect.");
        }
        if (!ModelState.IsValid) return View("AccountSettings", model);
        user.DisplayName = model.DisplayName.Trim();
        user.Email = model.Email.Trim();
        await _context.SaveChangesAsync();
        await _auditLogService.RecordAsync(User.Identity?.Name, "Account updated", "Administrator profile or password was updated.", "AdminUser", user.Id.ToString());
        TempData["SettingsSuccess"] = "Account settings saved.";
        return RedirectToAction(nameof(AccountSettings));
    }

    [HttpGet("Settings/System")]
    public async Task<IActionResult> SystemInformation()
    {
        ViewData["Title"] = "System information";
        var settings = await _settingsService.GetAsync();
        ViewBag.ApplicationName = settings.ApplicationName;
        ViewBag.TimeZone = settings.TimeZone;
        ViewBag.CurrentAdministrator = User.Identity?.Name;
        ViewBag.DatabaseConnected = await _context.Database.CanConnectAsync();
        ViewBag.Provider = _context.Database.ProviderName;
        ViewBag.Bookings = await _context.Bookings.CountAsync();
        ViewBag.Destinations = await _context.Destinations.CountAsync();
        ViewBag.Hotels = await _context.Hotels.CountAsync();
        ViewBag.Restaurants = await _context.Restaurants.CountAsync();
        ViewBag.Resorts = await _context.Resorts.CountAsync();
        ViewBag.Transport = await _context.TransportServices.CountAsync();
        return View("SystemInformation");
    }
}
