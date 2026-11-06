using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIKnowledgeEntry
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(4000)] public string Content { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Category { get; set; } = "General";
    public bool IsEnabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
