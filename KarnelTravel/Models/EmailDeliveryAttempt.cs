using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class EmailDeliveryAttempt
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
    public int? CustomerId { get; set; }
    public CustomerAccount? Customer { get; set; }

    [Required, EmailAddress, StringLength(256)]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string MessageType { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(2000)]
    public string? ProviderMessage { get; set; }

    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
