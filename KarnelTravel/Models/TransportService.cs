
namespace KarnelTravel.Models
{
    public class TransportService
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public string? VehicleType { get; set; }
        public string? StartingCity { get; set; }
        public string? DestinationCity { get; set; }

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public int? PassengerCapacity { get; set; }
        public decimal? StartingPrice { get; set; }

        public string? ContactPhone { get; set; }

        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? DestinationId { get; set; }
        public Destination? Destination { get; set; }
    }
}
