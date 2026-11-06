using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIConfiguration
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    [Required, StringLength(80)] public string AssistantName { get; set; } = "Karnel AI";
    [Required, StringLength(500)] public string WelcomeMessage { get; set; } = "I can help you explore published Karnel Travel listings and plan a trip.";
    [Required, StringLength(40)] public string ResponseTone { get; set; } = "Helpful";
    [Range(100, 10000)] public int MaximumMessageLength { get; set; } = 2000;
    [Range(1, 100)] public int ConversationMessageLimit { get; set; } = 30;
    [StringLength(2000)] public string SystemInstructions { get; set; } = "Use published database facts only. Never invent prices, availability, or listings.";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [StringLength(256)] public string? UpdatedBy { get; set; }
}
