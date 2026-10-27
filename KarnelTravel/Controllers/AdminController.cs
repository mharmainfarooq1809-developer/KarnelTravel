
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
using System.Text.RegularExpressions;
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

        if (!ValidatePropertyForm(model))
        {
            return View("PropertyForm", model);
        }

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

        await _context.SaveChangesAsync();

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
                        DestinationId = item.DestinationId
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
                        DestinationId = item.DestinationId
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
                        DestinationId = item.DestinationId
                    };
                    break;
                }

            default:
                return BadRequest("Invalid property category.");
        }

        ViewData["Title"] = $"Edit {model.Category}";
        ViewData["Section"] = "Stays";
        ViewData["PropertyId"] = id;

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

        if (!ValidatePropertyForm(model))
        {
            return View("PropertyForm", model);
        }

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

        await _context.SaveChangesAsync();

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
