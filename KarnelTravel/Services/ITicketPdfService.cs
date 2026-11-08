using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface ITicketPdfService
{
    byte[] Generate(Booking booking, BookingTicket ticket);
}
