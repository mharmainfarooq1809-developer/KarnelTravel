using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class SiteSetting
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Key { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Value { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}