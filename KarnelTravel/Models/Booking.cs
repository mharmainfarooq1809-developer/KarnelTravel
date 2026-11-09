using System;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class Booking
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [StringLength(80)]
    public string? IdempotencyKey { get; set; }

    [Required, StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(256)]
    public string? OwnerEmail { get; set; }
    public int? CustomerId { get; set; }
    public CustomerAccount? Customer { get; set; }

    [Required, StringLength(30)]
    public string ServiceType { get; set; } = string.Empty;

    public int? HotelId { get; set; }
    public int? RestaurantId { get; set; }
    public int? ResortId { get; set; }
    public int? TourPackageId { get; set; }
    public int? TourDepartureId { get; set; }
    public int? TransportServiceId { get; set; }
    public int? TransportRouteId { get; set; }
    public TransportRoute? TransportRoute { get; set; }

    [Required, StringLength(150)]
    public string ServiceName { get; set; } = string.Empty;

    [StringLength(150)]
    public string? TransportRouteName { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [StringLength(20)]
    public string? PreferredVisitTime { get; set; }

    public DateTime? ReturnDate { get; set; }

    [Range(1, 100)]
    public int Adults { get; set; } = 1;

    [Range(0, 100)]
    public int Children { get; set; }

    [Range(0, 100)]
    public int Rooms { get; set; }

    [Range(0, 100000000)]
    public decimal? EstimatedTotalPKR { get; set; }

    [Range(0, 100000000)]
    public decimal? TransportOneWayFarePKR { get; set; }

    [Range(0, 100000000)]
    public decimal? TransportReturnFarePKR { get; set; }

    [Range(0, 100000000)]
    public decimal? TransportDiscountPKR { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(1000)]
    public string? CustomerNotes { get; set; }

    [StringLength(1000)]
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(200)]
    public string? PickupLocation { get; set; }

    [StringLength(200)]
    public string? DropoffLocation { get; set; }

    [StringLength(20)]
    public string? TripType { get; set; }

    public TourDeparture? TourDeparture { get; set; }
    public ICollection<BookingTicket> Tickets { get; set; } = new List<BookingTicket>();
    public ICollection<BookingStatusHistory> StatusHistory { get; set; } = new List<BookingStatusHistory>();
    public ICollection<CustomerNotification> Notifications { get; set; } = new List<CustomerNotification>();
    public ICollection<EmailDeliveryAttempt> EmailDeliveryAttempts { get; set; } = new List<EmailDeliveryAttempt>();

    public static bool CanTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
            return true;

        return currentStatus switch
        {
            "Pending" => nextStatus is "Confirmed" or "Rejected" or "Cancelled",
            "Confirmed" => nextStatus is "Completed" or "Cancelled",
            _ => false
        };
    }

    public static bool CanAdminDelete(string? status) =>
        string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);
}
