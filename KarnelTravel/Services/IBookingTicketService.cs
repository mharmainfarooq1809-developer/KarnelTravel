using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IBookingTicketService
{
    Task<BookingTicket?> SyncForStatusAsync(Booking booking, CancellationToken cancellationToken = default);
    byte[] GeneratePdf(Booking booking, BookingTicket ticket);
}
