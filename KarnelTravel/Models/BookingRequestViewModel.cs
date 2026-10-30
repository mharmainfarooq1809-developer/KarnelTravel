
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public class BookingRequestViewModel
{
    [Required, StringLength(30)]
    public string ServiceType { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string CustomerName { get; set; } = "";

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = "";

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Travel date")]
    public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

    [DataType(DataType.Date)]
    [Display(Name = "Return date")]
    public DateTime? ReturnDate { get; set; }

    [Range(1, 100)]
    [Display(Name = "Adults / passengers")]
    public int Adults { get; set; } = 1;

    [Range(0, 100)]
    public int Children { get; set; }

    [Range(0, 100)]
    public int Rooms { get; set; }

    [StringLength(200)]
    [Display(Name = "Pickup location")]
    public string? PickupLocation { get; set; }

    [StringLength(200)]
    [Display(Name = "Drop-off location")]
    public string? DropoffLocation { get; set; }

    [StringLength(20)]
    [Display(Name = "Journey type")]
    public string? TripType { get; set; }

    [StringLength(1000)]
    [Display(Name = "Additional requests")]
    public string? CustomerNotes { get; set; }
}
