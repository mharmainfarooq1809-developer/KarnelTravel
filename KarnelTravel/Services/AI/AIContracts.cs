using KarnelTravel.Models;

namespace KarnelTravel.Services.AI;

public enum AIIntent
{
    Greeting, Help, DestinationSearch, HotelSearch, RestaurantSearch, ResortSearch, TransportSearch,
    GeneralTravelInformation, TravelRecommendation, TripPlanning, BudgetEstimation, DestinationComparison,
    BookingInquiry, ContactInquiry, BookingStatus, CompanyInformation, Unsupported
}

public interface IAICapabilityService
{
    Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default);
}

public sealed record AIEntities(string? Location, string? Category, decimal? MinimumBudget, decimal? MaximumBudget, int? DurationDays, int? Travelers, string? Preference, IReadOnlyList<string> ListingNames);
public sealed record AIRequest(string Message, string ConversationKey);
public sealed record AIResponse(string Text, AIIntent Intent, bool Supported, IReadOnlyList<TravelAiListing> Listings, string ConversationKey);
public sealed record AIActionResult(bool Success, string Message, object? Data = null);

public interface ILanguageModelProvider
{
    Task<string> GenerateAsync(string instruction, CancellationToken cancellationToken = default);
}

public interface IAIEngine
{
    Task<AIResponse> ProcessAsync(AIRequest request, CancellationToken cancellationToken = default);
}

public interface IAIActionService
{
    Task<AIActionResult> SearchAsync(string query, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<AIActionResult> CompareAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default);
}
