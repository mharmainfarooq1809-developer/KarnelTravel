using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class BookingStatusHistory
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required, StringLength(30)]
    public string PreviousStatus { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string NewStatus { get; set; } = string.Empty;

    [StringLength(128)]
    public string? ChangedByUserId { get; set; }

    [StringLength(30)]
    public string? ChangedByRole { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Reason { get; set; }
}
