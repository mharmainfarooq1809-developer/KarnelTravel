
using System;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public class AdminTourDepartureRowViewModel
{
    public int Id { get; set; }
    public int TourPackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;

    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }

    public int MaximumGroupSize { get; set; }
    public int AvailableSeats { get; set; }
    public int ReservedSeats { get; set; }
    public int ConfirmedBookings { get; set; }

    public bool IsActive { get; set; }
}

public class AdminTourDepartureFormViewModel
{
    public int Id { get; set; }

    public int TourPackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departure date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Departure date")]
    public DateTime DepartureDate { get; set; } = DateTime.Today.AddDays(7);

    [Required(ErrorMessage = "Return date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Return date")]
    public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(12);

    [Required]
    [Range(1, 500, ErrorMessage = "Group size must be between 1 and 500.")]
    [Display(Name = "Maximum group size")]
    public int MaximumGroupSize { get; set; } = 12;

    [Display(Name = "Active departure")]
    public bool IsActive { get; set; } = true;
}
