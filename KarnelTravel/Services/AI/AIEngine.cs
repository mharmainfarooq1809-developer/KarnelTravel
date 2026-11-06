using System.Text.RegularExpressions;
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services.AI;

public sealed class IntentDetector
{
    public AIIntent Detect(string text)
    {
        if (Regex.IsMatch(text, "\\b(hi|hello|hey|salam)\\b", RegexOptions.IgnoreCase)) return AIIntent.Greeting;
        if (Regex.IsMatch(text, "\\b(help|what can you do)\\b", RegexOptions.IgnoreCase)) return AIIntent.Help;
        if (Regex.IsMatch(text, "\\b(compare|versus| vs )\\b", RegexOptions.IgnoreCase)) return AIIntent.DestinationComparison;
        if (Regex.IsMatch(text, "\\b(plan|itinerary|days|trip)\\b", RegexOptions.IgnoreCase)) return AIIntent.TripPlanning;
        if (Regex.IsMatch(text, "\\b(budget|cost|price|expensive)\\b", RegexOptions.IgnoreCase)) return AIIntent.BudgetEstimation;
        if (Regex.IsMatch(text, "\\b(hotel|stay|accommodation)\\b", RegexOptions.IgnoreCase)) return AIIntent.HotelSearch;
        if (Regex.IsMatch(text, "\\b(restaurants?|food|eat|cuisine)\\b", RegexOptions.IgnoreCase)) return AIIntent.RestaurantSearch;
        if (Regex.IsMatch(text, "\\b(resort)\\b", RegexOptions.IgnoreCase)) return AIIntent.ResortSearch;
        if (Regex.IsMatch(text, "\\b(transport|car|vehicle|transfer)\\b", RegexOptions.IgnoreCase)) return AIIntent.TransportSearch;
        if (Regex.IsMatch(text, "\\b(recommend|suggest|best)\\b", RegexOptions.IgnoreCase)) return AIIntent.TravelRecommendation;
        if (Regex.IsMatch(text, "\\b(book|booking|reserve)\\b", RegexOptions.IgnoreCase)) return AIIntent.BookingInquiry;
        if (Regex.IsMatch(text, "\\b(contact|reach|email|phone)\\b", RegexOptions.IgnoreCase)) return AIIntent.ContactInquiry;
        if (Regex.IsMatch(text, "\\b(company|about Karnel|who are you)\\b", RegexOptions.IgnoreCase)) return AIIntent.CompanyInformation;
        if (Regex.IsMatch(text, "\\b(where|destination|visit|travel|tour)\\b", RegexOptions.IgnoreCase)) return AIIntent.DestinationSearch;
        return AIIntent.Unsupported;
    }
}

public sealed class EntityExtractor
{
    private static readonly Regex Budget = new("(?:under|below|max(?:imum)?|budget\\s*(?:is|of)?)\\s*(?:PKR\\s*)?(\\d[\\d,]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Days = new("(\\d+)\\s*days?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Travelers = new("(\\d+)\\s*(?:travellers|travelers|people|persons)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public AIEntities Extract(string text) => new(null, null, null, Parse(Budget.Match(text)), ParseInt(Days.Match(text)), ParseInt(Travelers.Match(text)), Regex.Match(text, "\\b(family|solo|group|couple)\\b", RegexOptions.IgnoreCase).Value, Array.Empty<string>());
    private static decimal? Parse(Match match) => match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var value) ? value : null;
    private static int? ParseInt(Match match) => match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : null;

    public AIEntities WithLocation(AIEntities entities, string? location) => entities with { Location = location };
}

public sealed class ConversationManager
{
    private readonly ApplicationDbContext _db;
    public ConversationManager(ApplicationDbContext db) => _db = db;
    public async Task SaveAsync(AIRequest request, AIResponse response, CancellationToken cancellationToken)
    {
        var conversation = await _db.AIConversations.FirstOrDefaultAsync(x => x.ConversationKey == request.ConversationKey, cancellationToken) ?? new AIConversation { ConversationKey = request.ConversationKey };
        if (conversation.Id == 0) _db.AIConversations.Add(conversation);
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.MessageCount += 2;
        conversation.Messages.Add(new AIMessage { Role = "user", Content = request.Message, Intent = response.Intent.ToString(), WasSupported = response.Supported });
        conversation.Messages.Add(new AIMessage { Role = "assistant", Content = response.Text, Intent = response.Intent.ToString(), WasSupported = response.Supported });
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ResponseBuilder
{
    public string Build(AIIntent intent, AIEntities entities, IReadOnlyList<TravelAiListing> listings, string? plan = null)
    {
        if (intent == AIIntent.Greeting) return "Welcome to Karnel Travel. Tell me a destination, stay, dining preference, or trip length and I’ll help you explore published options.";
        if (intent == AIIntent.Help) return "I can search published destinations, hotels, restaurants, resorts and transport, recommend options, compare listings, estimate known costs, and structure a trip plan.";
        if (intent == AIIntent.Unsupported) return "I can help with travel searches, recommendations, trip planning, budget estimates, comparisons, and booking inquiries. What destination or service would you like to explore?";
        if (plan != null) return plan;
        if (listings.Count == 0) return $"I could not find a published {intent.ToString().Replace("Search", "").ToLowerInvariant()} matching that request. Try another destination or service category.";
        return $"I found {listings.Count} published option(s) relevant to your request. Prices are shown only when stored in the Karnel Travel database.";
    }
}

public sealed class AIEngine : IAIEngine
{
    private readonly IntentDetector _intent = new();
    private readonly EntityExtractor _entities = new();
    private readonly ResponseBuilder _responses = new();
    private readonly ITravelSearchService _search;
    private readonly ITripPlannerService _planner;
    private readonly ConversationManager _conversation;
    private readonly IAICapabilityService _capabilities;
    private readonly ApplicationDbContext _db;
    public AIEngine(ApplicationDbContext db, ITravelSearchService search, ITripPlannerService planner, IAICapabilityService capabilities) { _db = db; _search = search; _planner = planner; _capabilities = capabilities; _conversation = new ConversationManager(db); }
    public async Task<AIResponse> ProcessAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var text = Regex.Replace(request.Message ?? string.Empty, "\\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(text)) return new AIResponse("Please tell me what you would like to explore.", AIIntent.Unsupported, false, Array.Empty<TravelAiListing>(), request.ConversationKey);
        var intent = _intent.Detect(text);
        var entities = _entities.Extract(text);
        var destination = await _db.Destinations.AsNoTracking().Where(x => x.IsPublished && text.Contains(x.Name)).Select(x => x.Name).FirstOrDefaultAsync(cancellationToken);
        entities = _entities.WithLocation(entities, destination);
        var capability = intent switch { AIIntent.DestinationSearch => "destination-search", AIIntent.HotelSearch => "hotel-search", AIIntent.RestaurantSearch => "restaurant-search", AIIntent.ResortSearch => "resort-search", AIIntent.TransportSearch => "transport-search", AIIntent.DestinationComparison => "destination-comparison", AIIntent.TripPlanning => "trip-planning", AIIntent.BudgetEstimation => "budget-estimation", AIIntent.BookingInquiry => "booking-inquiry", AIIntent.ContactInquiry => "contact-inquiry", AIIntent.BookingStatus => "booking-status", _ => null };
        if (capability != null && !await _capabilities.IsEnabledAsync(capability, cancellationToken))
        {
            var disabled = new AIResponse("That Karnel AI capability is currently disabled by the administrator. Please choose another travel request.", intent, false, Array.Empty<TravelAiListing>(), request.ConversationKey);
            await _conversation.SaveAsync(request, disabled, cancellationToken);
            return disabled;
        }
        var category = intent switch { AIIntent.HotelSearch => "hotel", AIIntent.RestaurantSearch => "restaurant", AIIntent.ResortSearch => "resort", AIIntent.TransportSearch => "transport", AIIntent.DestinationSearch or AIIntent.GeneralTravelInformation => "destination", _ => null };
        AISearchResult search;
        if (intent is AIIntent.Greeting or AIIntent.Help or AIIntent.Unsupported or AIIntent.TripPlanning)
            search = new AISearchResult(Array.Empty<TravelAiListing>(), 0, 1, 10);
        else
            search = await _search.SearchAsync(new AISearchRequest(text, category, entities.Location, entities.MaximumBudget), cancellationToken);
        var plan = intent == AIIntent.TripPlanning ? await _planner.PlanAsync(entities with { Location = entities.Location ?? text }, cancellationToken) : null;
        var response = new AIResponse(_responses.Build(intent, entities, search.Items, plan), intent, intent != AIIntent.Unsupported, search.Items, request.ConversationKey);
        await _conversation.SaveAsync(request, response, cancellationToken);
        return response;
    }
}
