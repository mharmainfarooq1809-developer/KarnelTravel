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
[Route("DemoPayment")]
public sealed class DemoPaymentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBookingStatusService _bookingStatusService;
    private readonly DemoCardValidationService _cardValidation = new();

    public DemoPaymentController(ApplicationDbContext db, IBookingStatusService bookingStatusService)
    {
        _db = db;
        _bookingStatusService = bookingStatusService;
    }

    [HttpGet("Checkout/{id:int}")]
    public async Task<IActionResult> Checkout(int id, CancellationToken cancellationToken)
    {
        if (!IsCustomerPortalUser())
            return Forbid();

        var booking = await OwnedBookingQuery(id).FirstOrDefaultAsync(cancellationToken);
        if (booking is null)
            return NotFound();

        var latestTransaction = await _db.DemoPaymentTransactions
            .AsNoTracking()
            .Where(x => x.BookingId == id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return View(new DemoPaymentCheckoutViewModel
        {
            Booking = booking,
            LatestTransaction = latestTransaction,
            Currency = "PKR"
        });
    }

    [HttpPost("Pay/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id, DemoPaymentFormViewModel form, CancellationToken cancellationToken)
    {
        if (!IsCustomerPortalUser())
            return Forbid();

        var booking = await OwnedBookingQuery(id).FirstOrDefaultAsync(cancellationToken);
        if (booking is null)
            return NotFound();

        if (booking.Status is not ("PendingPayment" or "PaymentFailed" or "Pending" or "Pending Admin Approval"))
            return BadRequest("This booking is no longer awaiting payment.");

        var successfulPaymentExists = await _db.DemoPaymentTransactions.AsNoTracking()
            .AnyAsync(x => x.BookingId == id && (x.PaymentStatus == "Successful" || x.PaymentStatus == "Paid (Demo)"), cancellationToken);
        if (successfulPaymentExists)
            return BadRequest("This booking already has a successful payment.");

        if (!booking.EstimatedTotalPKR.HasValue || booking.EstimatedTotalPKR.Value <= 0)
            return BadRequest("The server could not calculate a valid booking amount.");

        if (!TryValidateModel(form))
        {
            return View(nameof(Checkout), new DemoPaymentCheckoutViewModel
            {
                Booking = booking,
                Currency = "PKR",
                Form = form
            });
        }

        var card = _cardValidation.Validate(form.CardholderName, form.CardNumber, form.ExpiryDate, form.Cvv, form.PostalCode, DateTime.UtcNow);
        if (!card.IsValid)
        {
            ModelState.AddModelError(nameof(form.CardNumber), card.Error!);
            return View(nameof(Checkout), new DemoPaymentCheckoutViewModel
            {
                Booking = booking,
                Currency = "PKR",
                Form = form
            });
        }

        var now = DateTime.UtcNow;
        var previousStatus = booking.Status;
        var status = card.Outcome == "Successful" ? "Successful" : card.Outcome == "Cancelled" ? "Cancelled" : "Failed";
        var transaction = new DemoPaymentTransaction
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            TransactionReference = $"DEMO-TXN-{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            PaymentStatus = status,
            PaymentMethod = "Demo Card",
            CardBrand = card.CardBrand,
            MaskedCardNumber = card.MaskedCardNumber,
            IsSimulation = true,
            Amount = booking.EstimatedTotalPKR.Value,
            Currency = "PKR",
            FailureReason = status == "Successful" ? null : card.Outcome == "InsufficientFunds" ? "The configured demo card has insufficient funds." : card.Outcome == "Cancelled" ? "The demo transaction was cancelled." : "The configured demo card was declined.",
            RefundStatus = "Not Applicable",
            CreatedAt = now,
            ProcessedAt = now
        };

        await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.DemoPaymentTransactions.Add(transaction);
        var nextBookingStatus = status == "Successful" ? "Confirmed" : status == "Cancelled" ? "Cancelled" : "PaymentFailed";
        var changed = await _bookingStatusService.ChangeStatusAsync(
            booking,
            nextBookingStatus,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            "Customer",
            status == "Successful" ? "Successful demo payment." : transaction.FailureReason,
            cancellationToken);
        if (!changed)
            return BadRequest("The booking status could not be updated after payment.");
        await _db.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);

        return status == "Successful"
            ? RedirectToAction("Dashboard", "Customer")
            : RedirectToAction(nameof(Confirmation), new { id = transaction.Id });
    }

    [HttpPost("SimulateSuccess/{id:int}")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> SimulateSuccess(int id, CancellationToken cancellationToken) =>
        ProcessAsync(id, successful: true, cancellationToken);

    [HttpPost("SimulateFailure/{id:int}")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> SimulateFailure(int id, CancellationToken cancellationToken) =>
        ProcessAsync(id, successful: false, cancellationToken);

    [HttpPost("Cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        if (!IsCustomerPortalUser())
            return Forbid();
        var booking = await OwnedBookingQuery(id).FirstOrDefaultAsync(cancellationToken);
        if (booking is null)
            return NotFound();
        if (booking.Status is "Confirmed" or "Cancelled")
            return RedirectToAction(nameof(Checkout), new { id });
        booking.Status = "Cancelled";
        booking.UpdatedAt = DateTime.UtcNow;
        _db.DemoPaymentTransactions.Add(new DemoPaymentTransaction
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            TransactionReference = $"DEMO-TXN-{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            PaymentStatus = "Cancelled",
            PaymentMethod = "Demo Card",
            Amount = booking.EstimatedTotalPKR ?? 0m,
            Currency = "PKR",
            FailureReason = "Payment cancelled by the customer.",
            RefundStatus = "Not Applicable",
            IsSimulation = true,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Confirmation), new { id = await _db.DemoPaymentTransactions.Where(x => x.BookingId == id).OrderByDescending(x => x.Id).Select(x => x.Id).FirstAsync(cancellationToken) });
    }

    [HttpGet("Receipt/{id:int}")]
    public async Task<IActionResult> Receipt(int id, CancellationToken cancellationToken)
    {
        if (!IsCustomerPortalUser())
            return Forbid();
        var transaction = await _db.DemoPaymentTransactions.AsNoTracking().Include(x => x.Booking).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return transaction?.Booking is not null && Owns(transaction.Booking) && transaction.PaymentStatus == "Successful"
            ? View(transaction)
            : NotFound();
    }

    [HttpGet("Confirmation/{id:int}")]
    public async Task<IActionResult> Confirmation(int id, CancellationToken cancellationToken)
    {
        if (!IsCustomerPortalUser())
            return Forbid();

        var transaction = await _db.DemoPaymentTransactions
            .AsNoTracking()
            .Include(x => x.Booking)
            .Where(x => x.Id == id && x.Booking != null)
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction is null || !Owns(transaction.Booking!))
            return NotFound();

        return View(new DemoPaymentConfirmationViewModel
        {
            Booking = transaction.Booking!,
            Transaction = transaction
        });
    }

    private async Task<IActionResult> ProcessAsync(int id, bool successful, CancellationToken cancellationToken)
    {
        if (!IsCustomerPortalUser())
            return Forbid();

        var booking = await OwnedBookingQuery(id).FirstOrDefaultAsync(cancellationToken);
        if (booking is null)
            return NotFound();

        if (booking.Status is "Confirmed" or "Completed" or "Cancelled" or "Rejected")
            return BadRequest("This booking is no longer available for demo payment.");

        var paid = await _db.DemoPaymentTransactions
            .AsNoTracking()
            .AnyAsync(x => x.BookingId == id && x.PaymentStatus == "Paid (Demo)", cancellationToken);
        if (paid)
            return BadRequest("This booking already has a processed demo payment.");

        if (!booking.EstimatedTotalPKR.HasValue || booking.EstimatedTotalPKR.Value <= 0)
            return BadRequest("The booking amount is invalid or unavailable.");

        var amount = booking.EstimatedTotalPKR.Value;

        var transaction = new DemoPaymentTransaction
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            TransactionReference = $"DEMO-TXN-{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            PaymentStatus = successful ? "Paid (Demo)" : "Failed (Demo)",
            PaymentMethod = "Demo Card",
            Amount = amount,
            Currency = "PKR",
            FailureReason = successful ? null : "Simulated demo payment failure.",
            RefundStatus = "Not Applicable",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        _db.DemoPaymentTransactions.Add(transaction);

        if (successful)
        {
            bool changed;
            try
            {
                changed = await _bookingStatusService.ChangeStatusAsync(
                    booking,
                    "Pending Admin Approval",
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    User.IsInRole("Customer") ? "Customer" : "User",
                    "Demo payment submitted; awaiting administrator approval.",
                    cancellationToken);
            }
            catch (DbUpdateException)
            {
                return BadRequest("This booking already has a processed demo payment.");
            }

            if (!changed)
                return BadRequest("The booking cannot be submitted for approval.");
        }
        else
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return RedirectToAction(nameof(Confirmation), new { id = transaction.Id });
    }

    private IQueryable<Booking> OwnedBookingQuery(int id)
    {
        var customerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idValue)
            && User.IsInRole("Customer") ? idValue : (int?)null;
        if (customerId.HasValue)
            return _db.Bookings.Where(x => x.Id == id && x.CustomerId == customerId.Value);

        var email = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        return _db.Bookings.Where(x => x.Id == id && x.OwnerEmail == email);
    }

    private bool Owns(Booking booking)
    {
        var customerId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            && User.IsInRole("Customer") ? id : (int?)null;
        if (customerId.HasValue)
            return booking.CustomerId == customerId.Value;

        var email = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        return !string.IsNullOrWhiteSpace(email) && booking.OwnerEmail == email;
    }

    private bool IsCustomerPortalUser() => User.IsInRole("Customer") || User.IsInRole("User");
}
