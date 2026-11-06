using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIFeedback
{
    public long Id { get; set; }
    [Required, StringLength(80)] public string ConversationKey { get; set; } = string.Empty;
    public bool IsHelpful { get; set; }
    [StringLength(500)] public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
