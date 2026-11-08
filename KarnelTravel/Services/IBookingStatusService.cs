using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IBookingStatusService
{
    Task<bool> ChangeStatusAsync(Booking booking, string nextStatus, string? changedByUserId, string? changedByRole, string? reason, CancellationToken cancellationToken = default);
}
