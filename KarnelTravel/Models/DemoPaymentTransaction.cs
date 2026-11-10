using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class DemoPaymentTransaction
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int? CustomerId { get; set; }
    public CustomerAccount? Customer { get; set; }

    [Required, StringLength(40)]
    public string TransactionReference { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string PaymentStatus { get; set; } = "Pending";

    [Required, StringLength(30)]
    public string PaymentMethod { get; set; } = "Demo Card";

    [StringLength(20)]
    public string? CardBrand { get; set; }

    [StringLength(25)]
    public string? MaskedCardNumber { get; set; }

    public bool IsSimulation { get; set; } = true;

    [Range(typeof(decimal), "0", "100000000")]
    public decimal Amount { get; set; }

    [Required, StringLength(10)]
    public string Currency { get; set; } = "PKR";

    [StringLength(80)]
    public string? FailureReason { get; set; }

    [Required, StringLength(30)]
    public string RefundStatus { get; set; } = "Not Applicable";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
