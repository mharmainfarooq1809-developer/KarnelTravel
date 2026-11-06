using System.ComponentModel.DataAnnotations;
using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public sealed class KarnelAiPageViewModel
{
    public bool EngineAvailable { get; init; }
    public List<TravelAiListing> Listings { get; init; } = new();
    public List<string> QuickPrompts { get; init; } = new();
}

public sealed class KarnelAiFeedbackRequest
{
    [Required, StringLength(80)] public string ConversationKey { get; set; } = string.Empty;
    public bool IsHelpful { get; set; }
    [StringLength(500)] public string? Comment { get; set; }
}

public sealed class KarnelAiCompareRequest
{
    [Required, MinLength(2), MaxLength(4)] public List<int> ListingIds { get; set; } = new();
}

public sealed class KarnelAiChatRequest
{
    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;
    public string? ConversationKey { get; set; }
}

public sealed class KarnelAiChatResponse
{
    public bool Success { get; init; }
    public string Reply { get; init; } = string.Empty;
    public string? Error { get; init; }
    public IReadOnlyList<TravelAiListing> Sources { get; init; } = Array.Empty<TravelAiListing>();
    public string? Intent { get; init; }
    public string? ConversationKey { get; init; }
}

public sealed class KarnelAiPlannerRequest
{
    [Required, StringLength(120)] public string Destination { get; set; } = string.Empty;
    [Range(1, 60)] public int DurationDays { get; set; } = 3;
    [StringLength(80)] public string? Budget { get; set; }
    public string? ConversationKey { get; set; }
}

public sealed class KarnelAiPlannerResponse
{
    public bool Success { get; init; }
    public string Itinerary { get; init; } = string.Empty;
    public string? Error { get; init; }
    public IReadOnlyList<TravelAiListing> Sources { get; init; } = Array.Empty<TravelAiListing>();
    public string? ConversationKey { get; init; }
}
