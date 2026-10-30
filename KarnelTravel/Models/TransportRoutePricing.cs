
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KarnelTravel.Models;

public class TransportRoutePricing
{
    public int Id { get; set; }

    [Required]
    public int TransportServiceId { get; set; }

    public TransportService? TransportService { get; set; }

    [Required, StringLength(150)]
    public string PickupCity { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string DropoffCity { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 100000000)]
    public decimal OneWayFarePKR { get; set; }

    // Optional return fare. Null means use the one-way fare.
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 100000000)]
    public decimal? ReturnFarePKR { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
