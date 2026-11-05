using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class AdminAuditLog
{
    public long Id { get; set; }
    [StringLength(256)] public string? Administrator { get; set; }
    [Required, StringLength(100)] public string Action { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
    [StringLength(100)] public string? EntityType { get; set; }
    [StringLength(100)] public string? EntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Succeeded { get; set; } = true;
}
