using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public sealed class BookingStatusService : IBookingStatusService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending", "Confirmed", "Rejected", "Completed", "Cancelled"
    };
    private readonly ApplicationDbContext _db;
    private readonly IBookingTicketService _tickets;
    private readonly IBookingConfirmationEmailService _confirmationEmails;

    public BookingStatusService(
        ApplicationDbContext db,
        IBookingTicketService tickets,
        IBookingConfirmationEmailService confirmationEmails)
    {
        _db = db;
        _tickets = tickets;
        _confirmationEmails = confirmationEmails;
    }

    public async Task<bool> ChangeStatusAsync(Booking booking, string nextStatus, string? changedByUserId, string? changedByRole, string? reason, CancellationToken cancellationToken = default)
    {
        if (!AllowedStatuses.Contains(nextStatus) || !Booking.CanTransition(booking.Status, nextStatus))
            return false;

        var previousStatus = booking.Status;
        if (!string.Equals(previousStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            booking.Status = nextStatus;
            booking.StatusHistory.Add(new BookingStatusHistory
            {
                BookingId = booking.Id,
                PreviousStatus = previousStatus,
                NewStatus = nextStatus,
                ChangedByUserId = changedByUserId,
                ChangedByRole = changedByRole,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            });

            if (booking.CustomerId.HasValue)
            {
                _db.CustomerNotifications.Add(new CustomerNotification
                {
                    CustomerId = booking.CustomerId.Value,
                    BookingId = booking.Id,
                    Title = $"Booking {nextStatus}",
                    Message = $"Your booking {booking.ReferenceNumber} is now {nextStatus}.",
                    NotificationType = $"Booking{nextStatus}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (nextStatus is "Confirmed" or "Completed" or "Cancelled" or "Rejected")
            {
                var ticket = await _tickets.SyncForStatusAsync(booking, cancellationToken);
                if (nextStatus == "Confirmed" && ticket is not null)
                    await _confirmationEmails.SendAsync(booking, ticket, cancellationToken);
            }
        }

        booking.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
