
namespace KarnelTravel.Models
{
    public class ReportsViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }

        public decimal EstimatedBookingValue { get; set; }
        public int UncalculatedBookings { get; set; }

        public List<MonthlyBookingReport> MonthlyBookings { get; set; } = new();

        public List<ServiceBookingReport> ServicePerformance { get; set; } = new();
    }

    public class MonthlyBookingReport
    {
        public string Month { get; set; } = string.Empty;
        public int BookingCount { get; set; }
    }

    public class ServiceBookingReport
    {
        public string ServiceType { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal EstimatedValue { get; set; }
    }
}
