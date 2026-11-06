using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AICapability
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string Key { get; set; } = string.Empty;
    [Required, StringLength(160)] public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
