using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIAuditLog
{
    public long Id { get; set; }
    [StringLength(256)] public string? Administrator { get; set; }
    [Required, StringLength(100)] public string Action { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
    public bool Succeeded { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
