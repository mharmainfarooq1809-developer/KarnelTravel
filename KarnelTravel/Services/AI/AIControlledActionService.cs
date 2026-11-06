using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services.AI;

public sealed record AIInquiryRequest(string ConversationKey, string Name, string Email, string Message, bool Confirmed, string? Destination = null, string? ServiceNeeds = null, decimal? BudgetPKR = null);
public sealed record AIBookingStatusRequest(string ReferenceNumber, string Email, bool IdentityVerified);

public interface IAIControlledActionService
{
    Task<AIActionResult> PrepareContactInquiryAsync(AIInquiryRequest request, CancellationToken cancellationToken = default);
    Task<AIActionResult> SubmitContactInquiryAsync(AIInquiryRequest request, string? visitorIdentity, CancellationToken cancellationToken = default);
    Task<AIActionResult> PrepareBookingInquiryAsync(AIInquiryRequest request, CancellationToken cancellationToken = default);
    Task<AIActionResult> SubmitBookingInquiryAsync(AIInquiryRequest request, string? visitorIdentity, CancellationToken cancellationToken = default);
    Task<AIActionResult> GetBookingStatusAsync(AIBookingStatusRequest request, string? visitorIdentity, CancellationToken cancellationToken = default);
}

public sealed class AIControlledActionService : IAIControlledActionService
{
    private readonly ApplicationDbContext _db;
    private readonly IAICapabilityService _capabilities;
    public AIControlledActionService(ApplicationDbContext db, IAICapabilityService capabilities) { _db = db; _capabilities = capabilities; }

    public async Task<AIActionResult> PrepareContactInquiryAsync(AIInquiryRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _capabilities.IsEnabledAsync("contact-inquiry", cancellationToken)) return await FailureAsync("Contact inquiries are currently disabled.", "contact-inquiry-disabled", cancellationToken);
        return ValidateInquiry(request) ? new(true, "Contact inquiry prepared. Explicit confirmation is required before submission.") : await FailureAsync("The inquiry details are invalid.", "contact-inquiry-invalid", cancellationToken);
    }

    public async Task<AIActionResult> SubmitContactInquiryAsync(AIInquiryRequest request, string? visitorIdentity, CancellationToken cancellationToken = default)
    {
        if (!await _capabilities.IsEnabledAsync("contact-inquiry", cancellationToken)) return await FailureAsync("Contact inquiries are currently disabled.", "contact-inquiry-disabled", cancellationToken);
        if (!request.Confirmed) return await FailureAsync("Explicit confirmation is required before submitting an inquiry.", "contact-inquiry-confirmation-required", cancellationToken);
        if (!ValidateInquiry(request)) return await FailureAsync("The inquiry details are invalid.", "contact-inquiry-invalid", cancellationToken);
        _db.ContactInquiries.Add(new ContactInquiry { Name = request.Name.Trim(), Email = request.Email.Trim(), Category = "Karnel AI", Message = request.Message.Trim(), Status = "New" });
        await WriteAuditAsync(visitorIdentity, "AI contact inquiry submitted", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
        return new(true, "Your inquiry was submitted to Karnel Travel.");
    }

    public async Task<AIActionResult> PrepareBookingInquiryAsync(AIInquiryRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _capabilities.IsEnabledAsync("booking-inquiry", cancellationToken)) return await FailureAsync("Booking inquiries are currently disabled.", "booking-inquiry-disabled", cancellationToken);
        return ValidateInquiry(request) ? new(true, "Booking inquiry prepared. Review the details and explicitly confirm before submitting through the booking form.") : await FailureAsync("The booking inquiry details are invalid.", "booking-inquiry-invalid", cancellationToken);
    }

    public async Task<AIActionResult> SubmitBookingInquiryAsync(AIInquiryRequest request, string? visitorIdentity, CancellationToken cancellationToken = default)
    {
        if (!await _capabilities.IsEnabledAsync("booking-inquiry", cancellationToken)) return await FailureAsync("Booking inquiries are currently disabled.", "booking-inquiry-disabled", cancellationToken);
        if (!request.Confirmed) return await FailureAsync("Explicit confirmation is required before submitting a booking inquiry.", "booking-inquiry-confirmation-required", cancellationToken);
        if (!ValidateInquiry(request) || string.IsNullOrWhiteSpace(request.Destination) || string.IsNullOrWhiteSpace(request.ServiceNeeds)) return await FailureAsync("Destination and service needs are required.", "booking-inquiry-invalid", cancellationToken);
        var inquiry = new TripInquiry { ReferenceNumber = $"AI-{Guid.NewGuid():N}"[..15].ToUpperInvariant(), CustomerName = request.Name.Trim(), Phone = "AI follow-up", Email = request.Email.Trim(), Destinations = request.Destination.Trim(), ServiceNeeds = request.ServiceNeeds.Trim(), BudgetPKR = request.BudgetPKR, AdditionalRequirements = request.Message.Trim(), Status = "Pending" };
        _db.TripInquiries.Add(inquiry);
        await WriteAuditAsync(visitorIdentity, "AI booking inquiry submitted", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
        return new(true, "Your booking inquiry was submitted to Karnel Travel.", new { inquiry.ReferenceNumber });
    }

    public async Task<AIActionResult> GetBookingStatusAsync(AIBookingStatusRequest request, string? visitorIdentity, CancellationToken cancellationToken = default)
    {
        if (!await _capabilities.IsEnabledAsync("booking-status", cancellationToken)) return await FailureAsync("Booking-status retrieval is currently disabled.", "booking-status-disabled", cancellationToken);
        if (!request.IdentityVerified || string.IsNullOrWhiteSpace(visitorIdentity)) return await FailureAsync("Identity verification is required before retrieving booking status.", "booking-status-identity-required", cancellationToken);
        var reference = request.ReferenceNumber?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 30 || string.IsNullOrWhiteSpace(email)) return await FailureAsync("A valid booking reference and email address are required.", "booking-status-invalid", cancellationToken);
        var booking = await _db.Bookings.AsNoTracking().Where(x => x.ReferenceNumber == reference && x.OwnerEmail == email).Select(x => new { x.ReferenceNumber, x.Status, x.ServiceName, x.BookingDate }).SingleOrDefaultAsync(cancellationToken);
        if (booking == null) return await FailureAsync("No booking owned by the verified visitor matched those details.", "booking-status-not-found", cancellationToken);
        await WriteAuditAsync(visitorIdentity, "AI booking status retrieved", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
        return new(true, "Booking status retrieved.", booking);
    }

    private static bool ValidateInquiry(AIInquiryRequest request) => !string.IsNullOrWhiteSpace(request.ConversationKey) && request.ConversationKey.Length <= 80 && !string.IsNullOrWhiteSpace(request.Name) && request.Name.Length <= 100 && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email) && request.Message?.Trim().Length is > 0 and <= 2000;
    private async Task<AIActionResult> FailureAsync(string message, string action, CancellationToken cancellationToken) { await WriteAuditAsync(null, action, false, cancellationToken); await _db.SaveChangesAsync(cancellationToken); return new(false, message); }
    private Task WriteAuditAsync(string? actor, string action, bool succeeded, CancellationToken cancellationToken) { _db.AIAuditLogs.Add(new AIAuditLog { Administrator = actor, Action = action, Description = action, Succeeded = succeeded }); return Task.CompletedTask; }
}
