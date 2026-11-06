using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services.AI;

public sealed record AISearchRequest(string? Query, string? Category, string? Location, decimal? MaximumPrice, int Page = 1, int PageSize = 10);
public sealed record AISearchResult(IReadOnlyList<TravelAiListing> Items, int TotalCount, int Page, int PageSize);
public sealed record AIRecommendation(TravelAiListing Listing, int Score, string Explanation);
public sealed record AIBudgetEstimate(decimal KnownTotal, IReadOnlyList<string> UnavailableCategories);

public interface ITravelSearchService { Task<AISearchResult> SearchAsync(AISearchRequest request, CancellationToken cancellationToken = default); }
public interface IRecommendationService { Task<IReadOnlyList<AIRecommendation>> RecommendAsync(AIEntities entities, CancellationToken cancellationToken = default); }
public interface ITripPlannerService { Task<string> PlanAsync(AIEntities entities, CancellationToken cancellationToken = default); }
public interface IBudgetEstimatorService { Task<AIBudgetEstimate> EstimateAsync(AIEntities entities, CancellationToken cancellationToken = default); }

public sealed class TravelSearchService : ITravelSearchService
{
    private readonly ApplicationDbContext _db;
    public TravelSearchService(ApplicationDbContext db) => _db = db;

    public async Task<AISearchResult> SearchAsync(AISearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = (request.Query ?? string.Empty).Trim();
        var category = request.Category?.Trim().ToLowerInvariant();
        var location = request.Location?.Trim();
        var items = new List<TravelAiListing>();
        if (category is null or "destination") items.AddRange(await _db.Destinations.AsNoTracking().Where(x => x.IsPublished && (query == "" || x.Name.Contains(query) || (x.Region != null && x.Region.Contains(query))) && (location == null || (x.Region != null && x.Region.Contains(location)))).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).Select(x => new TravelAiListing { Type="Destination", Id=x.Id, Name=x.Name, Location=x.Region, Description=x.ShortDescription ?? x.Description, IsFeatured=x.IsFeatured, PriceLabel="Cost unavailable" }).ToListAsync(cancellationToken));
        if (category is null or "hotel") items.AddRange(await _db.Hotels.AsNoTracking().Where(x => x.IsPublished && (query == "" || x.Name.Contains(query) || (x.City != null && x.City.Contains(query))) && (location == null || (x.City != null && x.City.Contains(location))) && (!request.MaximumPrice.HasValue || (x.StartingPricePerNight.HasValue && x.StartingPricePerNight <= request.MaximumPrice))).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).Select(x => new TravelAiListing { Type="Hotel", Id=x.Id, Name=x.Name, Location=x.City, Description=x.ShortDescription ?? x.Description, IsFeatured=x.IsFeatured, Price=x.StartingPricePerNight, PriceLabel=x.StartingPricePerNight.HasValue ? $"From PKR {x.StartingPricePerNight.Value:N0} per night" : "Cost unavailable" }).ToListAsync(cancellationToken));
        if (category is null or "restaurant") items.AddRange(await _db.Restaurants.AsNoTracking().Where(x => x.IsPublished && (query == "" || x.Name.Contains(query) || (x.City != null && x.City.Contains(query))) && (location == null || (x.City != null && x.City.Contains(location))) && (!request.MaximumPrice.HasValue || (x.AverageMealPrice.HasValue && x.AverageMealPrice <= request.MaximumPrice))).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).Select(x => new TravelAiListing { Type="Restaurant", Id=x.Id, Name=x.Name, Location=x.City, Description=x.ShortDescription ?? x.Description, IsFeatured=x.IsFeatured, Price=x.AverageMealPrice, PriceLabel=x.AverageMealPrice.HasValue ? $"Average PKR {x.AverageMealPrice.Value:N0} per meal" : "Cost unavailable" }).ToListAsync(cancellationToken));
        if (category is null or "resort") items.AddRange(await _db.Resorts.AsNoTracking().Where(x => x.IsPublished && (query == "" || x.Name.Contains(query) || (x.CityOrRegion != null && x.CityOrRegion.Contains(query))) && (location == null || (x.CityOrRegion != null && x.CityOrRegion.Contains(location))) && (!request.MaximumPrice.HasValue || (x.StartingPricePerNight.HasValue && x.StartingPricePerNight <= request.MaximumPrice))).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).Select(x => new TravelAiListing { Type="Resort", Id=x.Id, Name=x.Name, Location=x.CityOrRegion, Description=x.ShortDescription ?? x.Description, IsFeatured=x.IsFeatured, Price=x.StartingPricePerNight, PriceLabel=x.StartingPricePerNight.HasValue ? $"From PKR {x.StartingPricePerNight.Value:N0} per night" : "Cost unavailable" }).ToListAsync(cancellationToken));
        if (category is null or "transport") items.AddRange(await _db.TransportServices.AsNoTracking().Where(x => x.IsPublished && (query == "" || x.Name.Contains(query) || (x.StartingCity != null && x.StartingCity.Contains(query)) || (x.DestinationCity != null && x.DestinationCity.Contains(query))) && (location == null || (x.StartingCity != null && x.StartingCity.Contains(location)) || (x.DestinationCity != null && x.DestinationCity.Contains(location))) && (!request.MaximumPrice.HasValue || (x.StartingPrice.HasValue && x.StartingPrice <= request.MaximumPrice))).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).Select(x => new TravelAiListing { Type="Transport", Id=x.Id, Name=x.Name, Location=x.DestinationCity, Description=x.Description, IsFeatured=x.IsFeatured, Price=x.StartingPrice, PriceLabel=x.StartingPrice.HasValue ? $"From PKR {x.StartingPrice.Value:N0}" : "Cost unavailable" }).ToListAsync(cancellationToken));
        var total = items.Count;
        return new AISearchResult(items.Skip((page - 1) * pageSize).Take(pageSize).ToList(), total, page, pageSize);
    }
}

public sealed class RecommendationService : IRecommendationService
{
    private readonly ITravelSearchService _search;
    private readonly ApplicationDbContext _db;
    public RecommendationService(ITravelSearchService search, ApplicationDbContext db) { _search = search; _db = db; }
    public async Task<IReadOnlyList<AIRecommendation>> RecommendAsync(AIEntities entities, CancellationToken cancellationToken = default)
    {
        var result = await _search.SearchAsync(new AISearchRequest(entities.Location, entities.Category, entities.Location, entities.MaximumBudget, 1, 20), cancellationToken);
        var settings = await _db.AIRecommendationSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken) ?? new AIRecommendationSettings();
        return result.Items.Select(x =>
        {
            var location = entities.Location != null && x.Location?.Contains(entities.Location, StringComparison.OrdinalIgnoreCase) == true;
            var budget = entities.MaximumBudget.HasValue && x.Price.HasValue && x.Price <= entities.MaximumBudget;
            var score = (location && settings.MatchDestination ? (int)settings.LocationWeight : 0)
                + (budget && settings.MatchBudget ? (int)settings.BudgetWeight : 0)
                + (x.IsFeatured && settings.PreferFeatured ? (int)settings.FeaturedWeight : 0)
                + (x.Type.Equals(entities.Category, StringComparison.OrdinalIgnoreCase) ? (int)settings.CategoryWeight : 0);
            return new AIRecommendation(x, score, location ? "Matches the requested published location." : x.Price.HasValue ? "Uses published listing data and a stored price." : "Matches published listing data; cost is unavailable.");
        }).OrderByDescending(x => x.Score).ThenBy(x => x.Listing.Name).ToList();
    }
}

public sealed class BudgetEstimatorService : IBudgetEstimatorService
{
    private readonly IRecommendationService _recommendations;
    public BudgetEstimatorService(IRecommendationService recommendations) => _recommendations = recommendations;
    public async Task<AIBudgetEstimate> EstimateAsync(AIEntities entities, CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendations.RecommendAsync(entities, cancellationToken);
        var days = entities.DurationDays ?? 1;
        var known = recommendations.Where(x => x.Listing.Price.HasValue).Select(x => x.Listing.Price!.Value).DefaultIfEmpty().First() * Math.Max(1, days);
        var unavailable = recommendations.Any(x => !x.Listing.Price.HasValue) ? new[] { "Some listing costs", "Transport", "Activities" } : new[] { "Transport", "Activities" };
        return new AIBudgetEstimate(known, unavailable);
    }
}

public sealed class TripPlannerService : ITripPlannerService
{
    private readonly IRecommendationService _recommendations;
    public TripPlannerService(IRecommendationService recommendations) => _recommendations = recommendations;
    public async Task<string> PlanAsync(AIEntities entities, CancellationToken cancellationToken = default)
    {
        var days = Math.Clamp(entities.DurationDays ?? 1, 1, 60);
        var results = await _recommendations.RecommendAsync(entities, cancellationToken);
        var stay = results.FirstOrDefault(x => x.Listing.Type is "Hotel" or "Resort");
        var food = results.FirstOrDefault(x => x.Listing.Type == "Restaurant");
        var lines = new List<string> { $"Trip plan for {entities.Location ?? "your selected destination"} ({days} day(s))", "", "Database-backed listings:" };
        if (stay != null) lines.Add($"Stay: {stay.Listing.Name} — {stay.Listing.PriceLabel}"); else lines.Add("Stay: no matching published hotel or resort found; cost unavailable.");
        if (food != null) lines.Add($"Food option: {food.Listing.Name} — {food.Listing.PriceLabel}"); else lines.Add("Food: no matching published restaurant found; cost unavailable.");
        lines.Add("");
        for (var day = 1; day <= days; day++) lines.Add($"Day {day}: Suggested itinerary structure for {entities.Location ?? "the selected destination"}. Activities are suggestions, not confirmed Karnel listings.");
        return string.Join(Environment.NewLine, lines);
    }
}

public sealed class AIActionService : IAIActionService
{
    private readonly ITravelSearchService _search;
    private readonly IAICapabilityService _capabilities;
    public AIActionService(ITravelSearchService search, IAICapabilityService capabilities) { _search = search; _capabilities = capabilities; }
    public async Task<AIActionResult> SearchAsync(string query, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length > 200 || page < 1 || pageSize is < 1 or > 50) return new(false, "The search request is invalid.");
        if (!await _capabilities.IsEnabledAsync("destination-search", cancellationToken)) return new(false, "Search is currently disabled.");
        return new(true, "Search completed.", await _search.SearchAsync(new AISearchRequest(query.Trim(), null, null, null, page, pageSize), cancellationToken));
    }
    public async Task<AIActionResult> CompareAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default)
    {
        if (!await _capabilities.IsEnabledAsync("destination-comparison", cancellationToken)) return new(false, "Comparison is currently disabled.");
        return ids.Count is >= 2 and <= 4 && ids.All(x => x > 0) ? new(true, "Comparison prepared.", ids) : new(false, "Compare between two and four valid listing identifiers.");
    }
}
