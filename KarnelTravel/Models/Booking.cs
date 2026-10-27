using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class Booking
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Required, StringLength(30)]
    public string ServiceType { get; set; } = string.Empty;

    public int? HotelId { get; set; }
    public int? RestaurantId { get; set; }
    public int? ResortId { get; set; }
    public int? TourPackageId { get; set; }
    public int? TransportServiceId { get; set; }

    [Required, StringLength(150)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    public DateTime BookingDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [Range(1, 100)]
    public int Adults { get; set; } = 1;

    [Range(0, 100)]
    public int Children { get; set; }

    [Range(0, 100)]
    public int Rooms { get; set; }

    [Range(0, 100000000)]
    public decimal? EstimatedTotalPKR { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(1000)]
    public string? CustomerNotes { get; set; }

    [StringLength(1000)]
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
