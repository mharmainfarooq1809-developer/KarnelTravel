using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KarnelTravel.Models;

public class TransportRouteVehicle
{
    public int Id { get; set; }

    public int TransportRouteId { get; set; }
    public TransportRoute? TransportRoute { get; set; }

    public int TransportServiceId { get; set; }
    public TransportService? TransportService { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Legacy route-pricing columns are retained for existing rows and migration compatibility.
    [Column("PickupCity"), StringLength(150)]
    public string? LegacyPickupCity { get; set; }

    [Column("DropoffCity"), StringLength(150)]
    public string? LegacyDropoffCity { get; set; }

    [Column("OneWayFarePKR", TypeName = "decimal(18,2)")]
    public decimal LegacyOneWayFarePKR { get; set; }

    [Column("ReturnFarePKR", TypeName = "decimal(18,2)")]
    public decimal? LegacyReturnFarePKR { get; set; }
}
