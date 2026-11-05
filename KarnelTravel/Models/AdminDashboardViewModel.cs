using System.Collections.Generic;

namespace KarnelTravel.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }

        public int TotalTripInquiries { get; set; }

        public int TotalContactInquiries { get; set; }

        public int TotalInquiries =>
            TotalTripInquiries + TotalContactInquiries;

        public int TotalListings { get; set; }

        public int TotalReviews { get; set; }

        public List<RecentBookingViewModel> RecentBookings { get; set; }
            = new();
    }
}