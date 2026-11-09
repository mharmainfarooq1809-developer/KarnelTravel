using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public class BookingRequestViewModel
{
    // =========================================================
    // SELECTED SERVICE
    // =========================================================

    [Required, StringLength(30)]
    public string ServiceType { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int? TransportVehicleId { get; set; }

    // =========================================================
    // TOUR BOOKING FIELDS
    // =========================================================

    [Display(Name = "Booking type")]
    public string TourBookingType { get; set; } = "Scheduled";

    [Display(Name = "Scheduled departure")]
    public int? TourDepartureId { get; set; }

    // =========================================================
    // CUSTOMER DETAILS
    // =========================================================

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string CustomerName { get; set; } = "";

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = "";

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    // =========================================================
    // BOOKING DATES
    // =========================================================

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Travel date")]
    public DateTime BookingDate { get; set; } =
        DateTime.Today.AddDays(1);

    [DataType(DataType.Date)]
    [Display(Name = "Return date")]
    public DateTime? ReturnDate { get; set; }

    // =========================================================
    // GUEST / PASSENGER DETAILS
    // =========================================================

    [Range(1, 100)]
    [Display(Name = "Adults / passengers")]
    public int Adults { get; set; } = 1;

    [Range(0, 100)]
    public int Children { get; set; } = 0;

    [Range(0, 100)]
    public int Rooms { get; set; } = 0;

    // =========================================================
    // TRANSPORT DETAILS
    // =========================================================

    [StringLength(200)]
    [Display(Name = "Pickup location")]
    public string? PickupLocation { get; set; }

    [StringLength(200)]
    [Display(Name = "Drop-off location")]
    public string? DropoffLocation { get; set; }

    [StringLength(20)]
    [Display(Name = "Journey type")]
    public string? TripType { get; set; }

    // =========================================================
    // RESTAURANT DETAILS
    // =========================================================

    [StringLength(20)]
    [Display(Name = "Preferred visit time")]
    public string? PreferredVisitTime { get; set; }

    // =========================================================
    // ADDITIONAL INFORMATION
    // =========================================================

    [StringLength(1000)]
    [Display(Name = "Additional requests")]
    public string? CustomerNotes { get; set; }

    // =========================================================
    // IDENTITY
    // =========================================================

    [StringLength(80)]
    public string? IdempotencyKey { get; set; }
}
