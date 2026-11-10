using System.Security.Claims;
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

[Authorize]
[Route("TripRequests")]
public sealed class TripRequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ISettingsService _settings;

    public TripRequestsController(ApplicationDbContext db, ISettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(string? destination, CancellationToken cancellationToken)
    {
        if (int.TryParse(destination, out var destinationId))
        {
            destination = await _db.Destinations
                .AsNoTracking()
                .Where(x => x.Id == destinationId && x.IsPublished)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var model = new CustomTripRequestViewModel
        {
            Destination = destination?.Trim() ?? string.Empty,
            DepartureDate = DateTime.Today.AddDays(1),
            PublishedDestinations = await PublishedDestinationNames(cancellationToken)
        };
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomTripRequestViewModel model, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetAsync();
        if (!settings.AllowContactSubmissions)
            return BadRequest("Custom trip requests are currently unavailable.");

        model.Destination = model.Destination.Trim();
        model.ServiceNeeds = model.ServiceNeeds.Trim();
        model.CustomerName = model.CustomerName.Trim();
        model.Phone = model.Phone.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.AdditionalRequirements = string.IsNullOrWhiteSpace(model.AdditionalRequirements) ? null : model.AdditionalRequirements.Trim();

        if (model.ReturnDate.HasValue && model.ReturnDate.Value.Date < model.DepartureDate!.Value.Date)
            ModelState.AddModelError(nameof(model.ReturnDate), "Return date cannot be before the departure date.");

        if (!ModelState.IsValid)
        {
            model.PublishedDestinations = await PublishedDestinationNames(cancellationToken);
            return View(model);
        }

        var idempotencyKey = string.IsNullOrWhiteSpace(model.IdempotencyKey)
            ? Guid.NewGuid().ToString("N")
            : model.IdempotencyKey.Trim();
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var existingInquiry = await _db.TripInquiries.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && x.OwnerEmail == ownerEmail, cancellationToken);
        if (existingInquiry != null)
            return RedirectToAction(nameof(Confirmation), new { reference = existingInquiry.ReferenceNumber });

        var destinationExists = await _db.Destinations.AsNoTracking().AnyAsync(x => x.IsPublished && x.Name == model.Destination, cancellationToken);
        if (!destinationExists)
            ModelState.AddModelError(nameof(model.Destination), "Select a published destination.");

        if (!ModelState.IsValid)
        {
            model.PublishedDestinations = await PublishedDestinationNames(cancellationToken);
            return View(model);
        }

        var reference = $"TRP-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var inquiry = new TripInquiry
        {
            IdempotencyKey = idempotencyKey,
            ReferenceNumber = reference,
            CustomerName = model.CustomerName,
            Phone = model.Phone,
            Email = model.Email,
            OwnerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant(),
            Destinations = model.Destination,
            ServiceNeeds = model.ServiceNeeds,
            BudgetPKR = model.BudgetPKR,
            DepartureDate = model.DepartureDate!.Value.Date,
            ReturnDate = model.ReturnDate?.Date,
            Adults = model.Adults,
            Children = model.Children,
            AdditionalRequirements = model.AdditionalRequirements,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.TripInquiries.Add(inquiry);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Confirmation), new { reference = inquiry.ReferenceNumber });
    }

    private Task<List<string>> PublishedDestinationNames(CancellationToken cancellationToken) =>
        _db.Destinations
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

    [HttpGet("Confirmation")]
    public async Task<IActionResult> Confirmation(string reference, CancellationToken cancellationToken)
    {
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var inquiry = await _db.TripInquiries.AsNoTracking().FirstOrDefaultAsync(x => x.ReferenceNumber == reference && x.OwnerEmail == ownerEmail, cancellationToken);
        return inquiry == null ? NotFound() : View(inquiry);
    }

    [HttpPost("{id:int}/AcceptOffer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptOffer(int id, CancellationToken cancellationToken)
    {
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var inquiry = await _db.TripInquiries.FirstOrDefaultAsync(x => x.Id == id && x.OwnerEmail == ownerEmail, cancellationToken);
        if (inquiry == null)
            return NotFound();
        if (inquiry.Status == "Accepted" && inquiry.ConvertedBookingId.HasValue)
            return RedirectToAction("Checkout", "DemoPayment", new { id = inquiry.ConvertedBookingId.Value });
        if (inquiry.Status != "Offer Prepared" || string.IsNullOrWhiteSpace(inquiry.ProposedOffer))
            return BadRequest("This request does not have an offer available for acceptance.");

        var booking = new Booking
        {
            TripInquiryId = inquiry.Id,
            ReferenceNumber = $"{(await _settings.GetAsync()).BookingReferencePrefix}-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            IdempotencyKey = $"trip-inquiry-{inquiry.Id}",
            CustomerName = inquiry.CustomerName,
            Phone = inquiry.Phone,
            Email = inquiry.Email,
            OwnerEmail = inquiry.OwnerEmail,
            ServiceType = "Custom Trip",
            ServiceName = inquiry.Destinations,
            ServiceNeeds = inquiry.ServiceNeeds,
            StartingCity = inquiry.StartingCity,
            BookingDate = inquiry.DepartureDate ?? DateTime.Today,
            ReturnDate = inquiry.ReturnDate,
            Adults = inquiry.Adults,
            Children = inquiry.Children,
            EstimatedTotalPKR = inquiry.BudgetPKR,
            CustomerNotes = inquiry.AdditionalRequirements,
            AdminNotes = inquiry.ProposedOffer,
            Status = "PendingPayment",
            CreatedAt = DateTime.UtcNow
        };
        _db.Bookings.Add(booking);
        inquiry.Status = "Accepted";
        inquiry.OfferAcceptedAt = DateTime.UtcNow;
        inquiry.ConvertedBookingId = booking.Id;
        await _db.SaveChangesAsync(cancellationToken);
        inquiry.ConvertedBookingId = booking.Id;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return RedirectToAction("Checkout", "DemoPayment", new { id = booking.Id });
    }

    [HttpPost("{id:int}/RejectOffer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOffer(int id, CancellationToken cancellationToken)
    {
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var inquiry = await _db.TripInquiries.FirstOrDefaultAsync(x => x.Id == id && x.OwnerEmail == ownerEmail, cancellationToken);
        if (inquiry == null)
            return NotFound();
        if (inquiry.Status != "Offer Prepared")
            return BadRequest("This request does not have an offer available for rejection.");
        inquiry.Status = "Declined";
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Confirmation), new { reference = inquiry.ReferenceNumber });
    }
}
