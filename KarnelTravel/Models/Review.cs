using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ReviewerName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        // A review belongs to exactly one listing.
        public int? DestinationId { get; set; }
        public Destination? Destination { get; set; }

        public int? HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        public int? RestaurantId { get; set; }
        public Restaurant? Restaurant { get; set; }

        public int? ResortId { get; set; }
        public Resort? Resort { get; set; }

        public int? TransportServiceId { get; set; }
        public TransportService? TransportService { get; set; }

        public int? TourPackageId { get; set; }
        public TourPackage? TourPackage { get; set; }
    }

    public enum ReviewStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}