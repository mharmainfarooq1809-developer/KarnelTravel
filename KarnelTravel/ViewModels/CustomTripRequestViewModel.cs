using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public sealed class CustomTripRequestViewModel
{
    [StringLength(80)]
    public string? IdempotencyKey { get; set; }

    [Required, StringLength(150)]
    public string Destination { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime? DepartureDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReturnDate { get; set; }

    [Range(1, 100)]
    public int Adults { get; set; } = 1;

    [Range(0, 100)]
    public int Children { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal? BudgetPKR { get; set; }

    [Required, StringLength(100)]
    public string ServiceNeeds { get; set; } = "Custom destination trip";

    [Required, StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [StringLength(2000)]
    public string? AdditionalRequirements { get; set; }
}
