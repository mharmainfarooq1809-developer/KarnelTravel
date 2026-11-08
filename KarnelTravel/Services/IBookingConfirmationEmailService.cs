using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IBookingConfirmationEmailService
{
    Task SendAsync(Booking booking, BookingTicket ticket, CancellationToken cancellationToken = default);
}
