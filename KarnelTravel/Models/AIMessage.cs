using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIMessage
{
    public long Id { get; set; }
    public long AIConversationId { get; set; }
    public AIConversation? Conversation { get; set; }
    [Required, StringLength(20)] public string Role { get; set; } = "user";
    [Required, StringLength(10000)] public string Content { get; set; } = string.Empty;
    [StringLength(60)] public string? Intent { get; set; }
    public bool WasSupported { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
