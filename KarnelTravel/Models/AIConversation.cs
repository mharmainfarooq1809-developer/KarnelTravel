using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIConversation
{
    public long Id { get; set; }
    [Required, StringLength(80)] public string ConversationKey { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [StringLength(40)] public string Status { get; set; } = "Active";
    public int MessageCount { get; set; }
    public ICollection<AIMessage> Messages { get; set; } = new List<AIMessage>();
}
