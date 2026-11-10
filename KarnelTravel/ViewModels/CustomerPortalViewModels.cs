using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public class CustomerDashboardViewModel
{
    public CustomerAccount Customer { get; set; } = null!;
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int PendingApprovalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public IReadOnlyList<Booking> RecentBookings { get; set; } = Array.Empty<Booking>();
    public IReadOnlyDictionary<int, BookingTicket> ActiveTickets { get; set; } = new Dictionary<int, BookingTicket>();
    public IReadOnlyList<TripInquiry> TripInquiries { get; set; } = Array.Empty<TripInquiry>();
}

public class CustomerBookingsViewModel
{
    public IReadOnlyList<Booking> Bookings { get; set; } = Array.Empty<Booking>();
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string Sort { get; set; } = "newest";
}

public class CustomerBookingDetailsViewModel
{
    public Booking Booking { get; set; } = null!;
    public IReadOnlyList<BookingStatusHistory> StatusHistory { get; set; } = Array.Empty<BookingStatusHistory>();
    public BookingTicket? ActiveTicket { get; set; }
}
