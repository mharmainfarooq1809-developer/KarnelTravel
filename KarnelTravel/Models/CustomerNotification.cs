using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class CustomerNotification
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public CustomerAccount Customer { get; set; } = null!;
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string NotificationType { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
