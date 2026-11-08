using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class BookingTicket
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required, StringLength(40)]
    public string TicketReference { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string VerificationTokenHash { get; set; } = string.Empty;

    [Required]
    public string QrText { get; set; } = string.Empty;

    [Required]
    public byte[] QrCodePng { get; set; } = Array.Empty<byte>();

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
