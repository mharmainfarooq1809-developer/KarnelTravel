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
    [Display(Name = "Arrival / booking date")]
    public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

    [DataType(DataType.Date)]
    [Display(Name = "Return / departure date")]
    public DateTime? ReturnDate { get; set; }

    [Range(1, 100)]
    public int Adults { get; set; } = 1;

    [Range(0, 100)]
    public int Children { get; set; }

    [Range(0, 100)]
    public int Rooms { get; set; }

    [StringLength(1000)]
    [Display(Name = "Additional requests")]
    public string? CustomerNotes { get; set; }
}
