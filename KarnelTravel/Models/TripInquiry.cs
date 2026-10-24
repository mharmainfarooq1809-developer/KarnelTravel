using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class TripInquiry
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string ReferenceNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(256)]
    public string? Email { get; set; }

    [Required, StringLength(1000)]
    public string Destinations { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ServiceNeeds { get; set; } = string.Empty;

    public decimal? BudgetPKR { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(120)]
    public string? StartingCity { get; set; }

    public DateTime? DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public int Adults { get; set; } = 1;
    public int Children { get; set; }

    [StringLength(2000)]
    public string? AdditionalRequirements { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}