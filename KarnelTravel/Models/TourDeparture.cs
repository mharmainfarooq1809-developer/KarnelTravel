
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class TourDeparture
{
    public int Id { get; set; }

    [Required]
    public int TourPackageId { get; set; }

    [Required]
    public DateTime DepartureDate { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }

    [Range(1, 500)]
    public int MaximumGroupSize { get; set; } = 12;

    [Range(0, 500)]
    public int AvailableSeats { get; set; } = 12;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public TourPackage? TourPackage { get; set; }

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}
