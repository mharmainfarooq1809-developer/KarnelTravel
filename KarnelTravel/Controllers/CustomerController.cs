using System.Security.Claims;
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

[Authorize]
[Route("Customer")]
public class CustomerController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICustomerAccountService _accounts;
    private readonly IPasswordHasher<AdminUser> _legacyHasher;

    public CustomerController(
        ApplicationDbContext db,
        ICustomerAccountService accounts,
        IPasswordHasher<AdminUser> legacyHasher)
    {
        _db = db;
        _accounts = accounts;
        _legacyHasher = legacyHasher;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        if (!IsPortalCustomer()) return Forbid();
        var customerId = GetCustomerId();
        var customer = customerId is int id
            ? await _accounts.FindByIdAsync(id, cancellationToken)
            : CreateLegacyCustomer();
        if (customer is null) return Challenge();

        var bookings = OwnedBookings();
        var recentBookings = await bookings.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(cancellationToken);
        var bookingIds = recentBookings.Select(x => x.Id).ToList();
        var activeTickets = await _db.BookingTickets.AsNoTracking()
            .Where(x => bookingIds.Contains(x.BookingId) && x.IsActive && x.RevokedAt == null)
            .ToDictionaryAsync(x => x.BookingId, cancellationToken);
        var model = new CustomerDashboardViewModel
        {
            Customer = customer,
            TotalBookings = await bookings.CountAsync(cancellationToken),
            PendingBookings = await bookings.CountAsync(x => x.Status == "Pending", cancellationToken),
            ConfirmedBookings = await bookings.CountAsync(x => x.Status == "Confirmed", cancellationToken),
            CompletedBookings = await bookings.CountAsync(x => x.Status == "Completed", cancellationToken),
            CancelledBookings = await bookings.CountAsync(x => x.Status == "Cancelled", cancellationToken),
            RecentBookings = recentBookings,
            ActiveTickets = activeTickets
        };
        return View(model);
    }

    [HttpGet("Bookings")]
    public async Task<IActionResult> Bookings(string? search, string? status, string sort = "newest", CancellationToken cancellationToken = default)
    {
        if (!IsPortalCustomer()) return Forbid();
        var query = OwnedBookings();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.ReferenceNumber.Contains(search.Trim()));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = string.Equals(sort, "oldest", StringComparison.OrdinalIgnoreCase) ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
        return View(new CustomerBookingsViewModel { Bookings = await query.ToListAsync(cancellationToken), Search = search, Status = status, Sort = sort });
    }

    [HttpGet("BookingDetails/{id:int}")]
    public async Task<IActionResult> BookingDetails(int id, CancellationToken cancellationToken)
    {
        if (!IsPortalCustomer()) return Forbid();
        var booking = await OwnedBookings().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (booking is null) return NotFound();
        return View(new CustomerBookingDetailsViewModel
        {
            Booking = booking,
            StatusHistory = await _db.BookingStatusHistory.AsNoTracking().Where(x => x.BookingId == id).OrderByDescending(x => x.ChangedAt).ToListAsync(cancellationToken),
            ActiveTicket = await _db.BookingTickets.AsNoTracking().FirstOrDefaultAsync(x => x.BookingId == id && x.IsActive && x.RevokedAt == null, cancellationToken)
        });
    }

    [HttpGet("Profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        if (GetCustomerId() is int customerId)
        {
            var customer = await _accounts.FindByIdAsync(customerId, cancellationToken);
            if (customer is null) return NotFound();
            return View(new CustomerProfileViewModel
            {
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                EmailConfirmed = customer.EmailConfirmed,
                CreatedAt = customer.CreatedAt
            });
        }

        var legacyUser = await GetLegacyUserAsync(cancellationToken);
        if (legacyUser is null) return Challenge();
        return View(new CustomerProfileViewModel
        {
            FullName = legacyUser.DisplayName,
            Email = legacyUser.Email,
            EmailConfirmed = true
        });
    }

    [HttpPost("Profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(CustomerProfileViewModel model, CancellationToken cancellationToken)
    {
        if (User.IsInRole("User"))
            ModelState.Remove(nameof(model.PhoneNumber));

        if (!ModelState.IsValid) return View(model);

        if (GetCustomerId() is int customerId)
        {
            if (!await _accounts.UpdateProfileAsync(customerId, model.FullName, model.PhoneNumber, cancellationToken)) return NotFound();
            if (!string.IsNullOrWhiteSpace(model.NewPassword) && !await _accounts.ChangePasswordAsync(customerId, model.CurrentPassword, model.NewPassword, cancellationToken))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is incorrect.");
                return View(model);
            }
        }
        else
        {
            var legacyUser = await GetLegacyUserAsync(cancellationToken);
            if (legacyUser is null) return Challenge();

            legacyUser.DisplayName = model.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(model.NewPassword) &&
                _legacyHasher.VerifyHashedPassword(legacyUser, legacyUser.PasswordHash, model.CurrentPassword) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is incorrect.");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
                legacyUser.PasswordHash = _legacyHasher.HashPassword(legacyUser, model.NewPassword);

            await _db.SaveChangesAsync(cancellationToken);
        }

        TempData["AccountMessage"] = "Your profile was updated.";
        return RedirectToAction(nameof(Profile));
    }

    private bool IsPortalCustomer() => User.IsInRole("Customer") || User.IsInRole("User");
    private int? GetCustomerId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && User.IsInRole("Customer") ? id : null;
    private IQueryable<Booking> OwnedBookings()
    {
        if (GetCustomerId() is int customerId)
            return _db.Bookings.AsNoTracking().Where(x => x.CustomerId == customerId);

        var email = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        return _db.Bookings.AsNoTracking().Where(x => x.OwnerEmail == email);
    }

    private CustomerAccount? CreateLegacyCustomer()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);
        return string.IsNullOrWhiteSpace(email) ? null : new CustomerAccount
        {
            FullName = name ?? email,
            Email = email,
            EmailConfirmed = true
        };
    }
    private Task<CustomerAccount?> GetCustomerAsync(CancellationToken cancellationToken) => GetCustomerId() is int id ? _accounts.FindByIdAsync(id, cancellationToken) : Task.FromResult<CustomerAccount?>(null);
    private Task<AdminUser?> GetLegacyUserAsync(CancellationToken cancellationToken) =>
        User.IsInRole("User") && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? _db.AdminUsers.AsTracking().FirstOrDefaultAsync(x => x.Id == id && x.Role == "User", cancellationToken)
            : Task.FromResult<AdminUser?>(null);
}
