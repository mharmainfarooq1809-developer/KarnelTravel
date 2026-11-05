
namespace KarnelTravel.Models
{
    public class RecentBookingViewModel
    {
        public string ReferenceNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
