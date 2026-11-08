using System.Text.RegularExpressions;
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services.AI;

public sealed class IntentDetector
{
    public AIIntent Detect(string text)
    {
        if (Regex.IsMatch(text, "^\\s*(hi|hello|hey|salam)(?:[,.!]?\\s+(?:my name is|i am|i'm)\\s+[A-Za-z][A-Za-z '-]*)?[!.?\\s]*$", RegexOptions.IgnoreCase))
            return Regex.IsMatch(text, "\\b(my name is|i am|i'm)\\b", RegexOptions.IgnoreCase) ? AIIntent.Introduction : AIIntent.Greeting;
        if (Regex.IsMatch(text, "\\b(my name is|i am|i'm)\\s+[A-Za-z][A-Za-z '-]*[.!?]?$", RegexOptions.IgnoreCase)) return AIIntent.Introduction;
        if (Regex.IsMatch(text, "\\b(what is|what's|do you know)\\s+my name\\b", RegexOptions.IgnoreCase)) return AIIntent.NameRecall;
        if (Regex.IsMatch(text, "^\\s*(thanks|thank you|thx|appreciate it)[!.?\\s]*$", RegexOptions.IgnoreCase)) return AIIntent.Gratitude;
        if (Regex.IsMatch(text, "^\\s*(okay|ok|alright|got it|great|sure|yes|no)[!.?\\s]*$", RegexOptions.IgnoreCase)) return AIIntent.Acknowledgement;
        if (Regex.IsMatch(text, "^\\s*(bye|goodbye|see you|talk to you later)[!.?\\s]*$", RegexOptions.IgnoreCase)) return AIIntent.Farewell;
        if (Regex.IsMatch(text, "\\b(can you clarify|please clarify|what do you mean|i don't understand|could you explain)\\b", RegexOptions.IgnoreCase)) return AIIntent.Clarification;
        if (Regex.IsMatch(text, "\\b(help|what can you do)\\b", RegexOptions.IgnoreCase)) return AIIntent.Help;
        if (Regex.IsMatch(text, "\\b(compare|versus|\\bvs\\b|difference between)\\b", RegexOptions.IgnoreCase) && (HasDestinationPair(text) || Regex.IsMatch(text, "\\b destinations?\\b", RegexOptions.IgnoreCase))) return AIIntent.DestinationComparison;
        if (Regex.IsMatch(text, "\\b(difference between|hotel and resort|hotel or resort|which is better.*(hotel|resort)|(hotel|resort).*which is better)\\b", RegexOptions.IgnoreCase)) return AIIntent.GeneralTravelInformation;
        if (Regex.IsMatch(text, "\\b(hotel|hotels|accommodation|stay)\\b", RegexOptions.IgnoreCase) && Regex.IsMatch(text, "\\b(price|cost|rate|per night|how much)\\b", RegexOptions.IgnoreCase)) return AIIntent.HotelPriceInquiry;
        if (Regex.IsMatch(text, "\\b(restaurant|restaurants|food|meal)\\b", RegexOptions.IgnoreCase) && Regex.IsMatch(text, "\\b(price|cost|rate|how much)\\b", RegexOptions.IgnoreCase)) return AIIntent.RestaurantPriceInquiry;
        if (Regex.IsMatch(text, "\\b(transport|fare|taxi|car|vehicle|transfer)\\b", RegexOptions.IgnoreCase) && Regex.IsMatch(text, "\\b(price|cost|fare|rate|how much)\\b", RegexOptions.IgnoreCase)) return AIIntent.TransportFareInquiry;
        if (Regex.IsMatch(text, "\\b(plan|itinerary|trip)\\b", RegexOptions.IgnoreCase)) return AIIntent.TripPlanning;
        if (Regex.IsMatch(text, "\\b(recommend|suggest|best)\\b", RegexOptions.IgnoreCase) && HasKnownDestination(text)) return AIIntent.TripPlanning;
        if (Regex.IsMatch(text, "\\b(budget|cost|price|expensive)\\b", RegexOptions.IgnoreCase)) return AIIntent.BudgetEstimation;
        if (Regex.IsMatch(text, "\\b(book|booking|reserve)\\b", RegexOptions.IgnoreCase)) return AIIntent.BookingInquiry;
        if (Regex.IsMatch(text, "\\b(contact|reach|email|phone)\\b", RegexOptions.IgnoreCase)) return AIIntent.ContactInquiry;
        if (Regex.IsMatch(text, "\\b(company|about Karnel|who are you)\\b", RegexOptions.IgnoreCase)) return AIIntent.CompanyInformation;
        if (Regex.IsMatch(text, "\\b(show|find|search|list|available)\\b.*\\b(hotel|hotels|stay|accommodation)\\b|\\b(hotel|hotels)\\b.*\\b(in|near|at)\\b", RegexOptions.IgnoreCase)) return AIIntent.HotelSearch;
        if (Regex.IsMatch(text, "\\b(show|find|search|list|available)\\b.*\\b(restaurants?|food)\\b|\\b(restaurants?)\\b.*\\b(in|near|at)\\b", RegexOptions.IgnoreCase)) return AIIntent.RestaurantSearch;
        if (Regex.IsMatch(text, "\\b(show|find|search|list|available)\\b.*\\bresorts?\\b|\\bresorts?\\b.*\\b(in|near|at)\\b", RegexOptions.IgnoreCase)) return AIIntent.ResortSearch;
        if (Regex.IsMatch(text, "\\b(show|find|search|list|available)\\b.*\\b(transport|car|vehicle|transfer)\\b|\\b(transport|cars?|vehicles?|transfers?)\\b.*\\b(in|between|from|to)\\b", RegexOptions.IgnoreCase)) return AIIntent.TransportSearch;
        if (Regex.IsMatch(text, "\\b(show|find|search|list|available)\\b.*\\b(destinations?|places|tours)\\b", RegexOptions.IgnoreCase)) return AIIntent.DestinationSearch;
        if (Regex.IsMatch(text, "\\b(natural beauty|(?:main|popular) attractions?|best-known travel season|travel seasons?|tell me about|information about|what is .* like|where is .* located|known for)\\b", RegexOptions.IgnoreCase)) return AIIntent.DestinationInformation;
        if (Regex.IsMatch(text, "\\b(explain|what should travellers?|what should travelers?|travel safety|travel preparation|road conditions|travel advisories)\\b", RegexOptions.IgnoreCase)) return AIIntent.GeneralTravelInformation;
        if (Regex.IsMatch(text, "\\b(recommend|suggest|best)\\b", RegexOptions.IgnoreCase)) return AIIntent.DestinationRecommendation;
        if (Regex.IsMatch(text, "\\b(days?|duration)\\b", RegexOptions.IgnoreCase) && !HasKnownDestination(text)) return AIIntent.DestinationRecommendation;
        if (Regex.IsMatch(text, "\\b(where|destination|visit|travel|tour)\\b", RegexOptions.IgnoreCase)) return AIIntent.DestinationInformation;
        if (Regex.IsMatch(text, "^\\s*(how are you|nice to meet you|who am i|good morning|good evening)[!.?\\s]*$", RegexOptions.IgnoreCase)) return AIIntent.GeneralConversation;
        return AIIntent.Unsupported;
    }

    private static bool HasKnownDestination(string text) => Regex.IsMatch(text, "\\b(hunza(?: valley)?|skardu(?: valley)?|lahore|islamabad|naran|kaghan|swat|murree)\\b", RegexOptions.IgnoreCase);
    private static bool HasDestinationPair(string text) => Regex.Matches(text, "\\b(hunza(?: valley)?|skardu(?: valley)?|lahore|islamabad|naran|kaghan|swat|murree)\\b", RegexOptions.IgnoreCase).Count >= 2;
}

public sealed class EntityExtractor
{
    private static readonly Regex Budget = new("(?:under|below|max(?:imum)?|budget\\s*(?:is|of)?)\\s*(?:PKR\\s*)?(\\d[\\d,]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Days = new("(\\d+|one|two|three|four|five|six|seven|eight|nine|ten)(?:\\s*-\\s*|\\s+)days?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Travelers = new("(\\d+)\\s*(?:travellers|travelers|people|persons)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Destination = new("\\b(hunza(?: valley)?|skardu(?: valley)?|lahore|islamabad|naran|kaghan|swat|murree)\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public AIEntities Extract(string text)
    {
        var destinations = Destination.Matches(text).Select(x => NormalizeDestination(x.Value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(null, null, null, Parse(Budget.Match(text)), ParseInt(Days.Match(text)), ParseInt(Travelers.Match(text)), Regex.Match(text, "\\b(family|solo|group|couple)\\b", RegexOptions.IgnoreCase).Value, Array.Empty<string>(), destinations.FirstOrDefault(), destinations.Skip(1).FirstOrDefault(), destinations);
    }
    private static decimal? Parse(Match match) => match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var value) ? value : null;
    private static int? ParseInt(Match match)
    {
        if (!match.Success) return null;
        if (int.TryParse(match.Groups[1].Value, out var value)) return value;
        return match.Groups[1].Value.ToLowerInvariant() switch
        {
            "one" => 1, "two" => 2, "three" => 3, "four" => 4, "five" => 5,
            "six" => 6, "seven" => 7, "eight" => 8, "nine" => 9, "ten" => 10,
            _ => null
        };
    }

    public AIEntities WithLocation(AIEntities entities, string? location) => entities with { Location = location };
    private static string NormalizeDestination(string value) => value.Replace(" Valley", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
}

public sealed class ConversationManager
{
    private readonly ApplicationDbContext _db;
    public ConversationManager(ApplicationDbContext db) => _db = db;
    public async Task<AIConversationContext> GetContextAsync(string conversationKey, string? ownerEmail, bool isAdmin, string? anonymousOwnerKey, CancellationToken cancellationToken)
    {
        var messages = await _db.AIMessages.AsNoTracking()
            .Where(x => x.Conversation != null && x.Conversation.ConversationKey == conversationKey && x.Role == "user" &&
                (isAdmin || x.Conversation.OwnerEmail == ownerEmail || (x.Conversation.IsAnonymous && x.Conversation.AnonymousOwnerKey == anonymousOwnerKey)))
            .OrderByDescending(x => x.Id)
            .Take(12)
            .Select(x => x.Content)
            .ToListAsync(cancellationToken);
        var name = messages.Select(ExtractName).FirstOrDefault(x => x != null);
        var destination = messages.SelectMany(x => DestinationNames().Where(d => Regex.IsMatch(x, $"\\b{Regex.Escape(d)}\\b", RegexOptions.IgnoreCase))).FirstOrDefault();
        return new AIConversationContext(name, destination);
    }
    public async Task SaveAsync(AIRequest request, AIResponse response, CancellationToken cancellationToken)
    {
        var conversation = await _db.AIConversations.FirstOrDefaultAsync(x => x.ConversationKey == request.ConversationKey &&
            (request.IsAdmin || x.OwnerEmail == request.OwnerEmail || (x.IsAnonymous && x.AnonymousOwnerKey == request.AnonymousOwnerKey)), cancellationToken)
            ?? new AIConversation { ConversationKey = request.ConversationKey, OwnerEmail = request.OwnerEmail, AnonymousOwnerKey = request.AnonymousOwnerKey, IsAnonymous = string.IsNullOrWhiteSpace(request.OwnerEmail) };
        if (conversation.Id == 0) _db.AIConversations.Add(conversation);
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.MessageCount += 2;
        conversation.Messages.Add(new AIMessage { Role = "user", Content = request.Message, Intent = response.Intent.ToString(), WasSupported = response.Supported });
        conversation.Messages.Add(new AIMessage { Role = "assistant", Content = response.Text, Intent = response.Intent.ToString(), WasSupported = response.Supported });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string? ExtractName(string message)
    {
        var match = Regex.Match(message, "\\b(?:my name is|i am|i'm)\\s+([A-Za-z][A-Za-z'-]*)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static IEnumerable<string> DestinationNames() => new[] { "Hunza", "Skardu", "Lahore", "Islamabad", "Naran", "Kaghan", "Swat", "Murree" };
}

public sealed class ResponseBuilder
{
    private static readonly Regex InternalInstruction = new("\\b(important test instruction|internal (?:ai|administrative|admin)|testing instruction|retrieval metadata|do not reveal|if a traveller asks|if a traveler asks|mention at least|use this approved knowledge|do not invent)\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Build(AIIntent intent, AIEntities entities, IReadOnlyList<TravelAiListing> listings, string? plan = null, IReadOnlyList<AIKnowledgeMatch>? knowledge = null, string? question = null, AIConversationContext? context = null)
    {
        if (intent == AIIntent.Greeting) return "Welcome to Karnel Travel. Tell me a destination, stay, dining preference, or trip length and I’ll help you explore published options.";
        if (intent == AIIntent.Introduction) return $"Nice to meet you, {ExtractName(question) ?? "traveller"}! How can I help you plan your next journey?";
        if (intent == AIIntent.NameRecall) return context?.UserName is { Length: > 0 } name ? $"Your name is {name}." : "I don't have your name in this conversation yet. What should I call you?";
        if (intent == AIIntent.Gratitude) return "You're welcome! I'm happy to help with your journey.";
        if (intent == AIIntent.Acknowledgement) return "Great. What would you like to explore next?";
        if (intent == AIIntent.Farewell) return "Goodbye! I hope you have a wonderful journey.";
        if (intent == AIIntent.Clarification) return "Of course. Which part would you like me to clarify?";
        if (intent == AIIntent.GeneralConversation) return "I'm doing well and ready to help you discover a journey. What would you like to explore?";
        if (intent == AIIntent.Help) return "I can search published destinations, hotels, restaurants, resorts and transport, recommend options, compare listings, estimate known costs, and structure a trip plan.";
        if (plan != null) return plan;
        if (intent == AIIntent.DestinationRecommendation)
            return listings.Count == 0 ? "I couldn't find a suitable published destination recommendation yet. Tell me your preferred region, budget, or travel style and I can narrow the options." : $"Based on published Karnel destinations, consider:\n{string.Join("\n", listings.Select(x => $"- {x.Name}{(string.IsNullOrWhiteSpace(x.Location) ? string.Empty : $" ({x.Location})")}"))}\n\nChoose a destination and I can prepare a {entities.DurationDays ?? 1}-day suggested plan. No booking has been made.";
        if (intent == AIIntent.DestinationComparison)
            return BuildComparison(knowledge);
        if (intent == AIIntent.GeneralTravelInformation && IsAccommodationComparison(question))
            return "Hotels generally focus on room-based accommodation and core services. Resorts usually combine accommodation with more on-site leisure facilities and activities. The better choice depends on your budget, desired convenience, and whether you want to stay near planned activities.";
        if (knowledge is { Count: > 0 } && (intent is AIIntent.GeneralTravelInformation or AIIntent.DestinationInformation))
        {
            var answer = BuildKnowledgeAnswer(question ?? string.Empty, knowledge);
            return listings.Count == 0
                ? answer
                : $"{answer}\n\nI also found {listings.Count} published option(s) relevant to your request. Prices are shown only when stored in the Karnel Travel database.";
        }
        if (intent == AIIntent.DestinationInformation)
            return $"I don't currently have enough approved Karnel information{SubjectSuffix(question)} to provide a reliable destination overview. You can explore the published destinations and listings available in Karnel.";
        if (intent == AIIntent.GeneralTravelInformation)
            return "I don't currently have enough approved Karnel information to answer that reliably. You can explore the published destinations and listings available in Karnel.";
        if (intent == AIIntent.Unsupported) return "I can help with destinations, published hotels, resorts, restaurants, transport, trip planning, packages, and booking guidance. What would you like to explore?";
        if (listings.Count == 0) return EmptySearchMessage(intent, entities.Location);
        return $"I found {listings.Count} published option(s) relevant to your request. Prices are shown only when stored in the Karnel Travel database.";
    }
    private static string? ExtractName(string? question)
    {
        if (question == null) return null;
        var match = Regex.Match(question, "\\b(?:my name is|i am|i'm)\\s+([A-Za-z][A-Za-z'-]*)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string EmptySearchMessage(AIIntent intent, string? location)
    {
        var category = intent switch { AIIntent.HotelSearch or AIIntent.HotelPriceInquiry => "hotel", AIIntent.RestaurantSearch or AIIntent.RestaurantPriceInquiry => "restaurant", AIIntent.TransportSearch or AIIntent.TransportFareInquiry => "transport", AIIntent.ResortSearch => "resort", _ => "service" };
        return $"I couldn't find published {category} information{(string.IsNullOrWhiteSpace(location) ? string.Empty : $" for {location}")} at the moment. I won't invent prices or availability.";
    }

    private static string BuildComparison(IReadOnlyList<AIKnowledgeMatch>? knowledge)
    {
        if (knowledge is not { Count: > 0 }) return "I couldn't find approved knowledge for both destinations, so I can't make a reliable comparison.";
        return string.Join("\n\n", knowledge.Select(x => $"{DisplayTitle(x.Entry.Title)}\n{SelectFacts(string.Empty, x.Entry)}"));
    }

    private static string BuildKnowledgeAnswer(string question, IReadOnlyList<AIKnowledgeMatch> knowledge)
    {
        var answers = knowledge.Select(match => SelectFacts(question, match.Entry)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Take(5).ToList();
        return answers.Count == 0
            ? "I don't currently have enough approved Karnel information to answer that reliably. You can explore the published destinations and listings available in Karnel."
            : string.Join("\n\n", answers);
    }

    private static string SelectFacts(string question, AIKnowledgeEntry entry)
    {
        var safeContent = Regex.Split(entry.Content, "\\b(important test instruction|internal (?:ai|administrative|admin)|testing instruction|retrieval metadata)\\b", RegexOptions.IgnoreCase)[0];
        var lines = Regex.Split(safeContent, "\\r?\\n").Select(x => Regex.Replace(x.Trim(), "^[-•*]+\\s*", string.Empty)).Where(x => x.Length > 0 && !InternalInstruction.IsMatch(x)).ToList();
        var lowerQuestion = question.ToLowerInvariant();
        if (Regex.IsMatch(lowerQuestion, "\\b(season|seasons|best time|when)\\b"))
        {
            var season = lines.FirstOrDefault(x => Regex.IsMatch(x, "\\bseasons?\\b", RegexOptions.IgnoreCase));
            var safety = lines.SelectMany(SplitSentences).FirstOrDefault(x => Regex.IsMatch(x, "\\b(weather|road|advis|conditions)\\b", RegexOptions.IgnoreCase));
            if (season != null)
            {
                var match = Regex.Match(season, "(?:seasons?|travel seasons?)\\s+(?:are|include)\\s+([^.;]+)", RegexOptions.IgnoreCase);
                var answer = match.Success ? $"{Capitalize(match.Groups[1].Value.Trim())} are the best-known seasons for visiting {entry.Title.Replace("AI Knowledge Test", string.Empty).Trim(' ', '-', '–')}." : CleanFact(season);
                return safety == null ? answer : $"{answer} {CleanFact(safety)}";
            }
        }

        if (Regex.IsMatch(lowerQuestion, "\\b(attractions?|see|visit)\\b"))
        {
            var headingIndex = lines.FindIndex(x => Regex.IsMatch(x, "\\battractions?\\b", RegexOptions.IgnoreCase));
            if (headingIndex >= 0)
            {
                var heading = lines[headingIndex];
                var inline = Regex.Match(heading, "^(.+?attractions?)\\s*:\\s*(.+)$", RegexOptions.IgnoreCase);
                var attractions = inline.Success
                    ? Regex.Split(inline.Groups[2].Value, ",|\\s+and\\s+").Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                    : lines.Skip(headingIndex + 1).TakeWhile(x => !Regex.IsMatch(x, "\\b(season|weather|road|advis)\\b", RegexOptions.IgnoreCase)).Where(x => !x.EndsWith(':')).ToList();
                if (inline.Success) heading = inline.Groups[1].Value;
                if (attractions.Count > 0) return $"{heading.TrimEnd(':')}:\n{string.Join("\n", attractions.Select(x => $"- {x.TrimEnd('.', ',')}"))}";
            }
        }

        var relevant = lines.Where(x => Regex.IsMatch(x, "\\b(natural|beauty|scenery|landscape|mountain|culture|fort|lake|viewpoint|weather|road|advis|introduction|known)\\b", RegexOptions.IgnoreCase)).Select(CleanFact).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (relevant.Count == 0) relevant = lines.Select(CleanFact).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var result = string.Join(" ", relevant);
        var inlineAttractions = Regex.Match(result, "^(.+?attractions?)\\s*:\\s*(.+?)(?=\\s+(?:the best-known|travel conditions|visitors should|travel advisories)|$)", RegexOptions.IgnoreCase);
        if (inlineAttractions.Success)
        {
            var items = Regex.Split(inlineAttractions.Groups[2].Value, ",|\\s+and\\s+").Where(x => !string.IsNullOrWhiteSpace(x));
            result = $"{inlineAttractions.Groups[1].Value.TrimEnd(':')}:\n{string.Join("\n", items.Select(x => $"- {x.TrimEnd('.', ',')}"))}" + result[(inlineAttractions.Index + inlineAttractions.Length)..];
        }

        return result;
    }

    private static string CleanFact(string value) => InternalInstruction.Replace(value, string.Empty).Trim();
    private static string DisplayTitle(string value) => Regex.Replace(value, "\\s*[-–]\\s*AI Knowledge Test\\s*$", string.Empty, RegexOptions.IgnoreCase).Trim();
    private static IEnumerable<string> SplitSentences(string value) => Regex.Split(value, "(?<=[.!?])\\s+").Where(x => x.Length > 0);
    private static string Capitalize(string value) => value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
    private static bool IsAccommodationComparison(string? question) => question != null && Regex.IsMatch(question, "\\b(difference between|hotel and resort|hotel or resort|which is better)\\b", RegexOptions.IgnoreCase);
    private static string SubjectSuffix(string? question)
    {
        var match = question == null ? null : Regex.Match(question, "\\b(?:about|of|visiting|in)\\s+([A-Za-z][A-Za-z -]{1,40})", RegexOptions.IgnoreCase);
        var subject = match?.Success == true ? match.Groups[1].Value.Trim(' ', '.', '?', '!') : string.Empty;
        return string.IsNullOrWhiteSpace(subject) ? string.Empty : $" about {subject}";
    }
}

public sealed class AIEngine : IAIEngine
{
    private readonly IntentDetector _intent = new();
    private readonly EntityExtractor _entities = new();
    private readonly ResponseBuilder _responses = new();
    private readonly ITravelSearchService _search;
    private readonly ITripPlannerService _planner;
    private readonly IAIKnowledgeService _knowledge;
    private readonly ConversationManager _conversation;
    private readonly IAICapabilityService _capabilities;
    private readonly ApplicationDbContext _db;
    public AIEngine(ApplicationDbContext db, ITravelSearchService search, ITripPlannerService planner, IAICapabilityService capabilities, IAIKnowledgeService knowledge) { _db = db; _search = search; _planner = planner; _capabilities = capabilities; _knowledge = knowledge; _conversation = new ConversationManager(db); }
    public async Task<AIResponse> ProcessAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var text = Regex.Replace(request.Message ?? string.Empty, "\\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(text)) return new AIResponse("Please tell me what you would like to explore.", AIIntent.Unsupported, false, Array.Empty<TravelAiListing>(), request.ConversationKey);
        var context = await _conversation.GetContextAsync(request.ConversationKey, request.OwnerEmail, request.IsAdmin, request.AnonymousOwnerKey, cancellationToken);
        var intent = _intent.Detect(text);
        var contextualText = string.IsNullOrWhiteSpace(context.LastDestination) || !Regex.IsMatch(text, "\\b(there|that destination|this destination)\\b", RegexOptions.IgnoreCase)
            ? text
            : $"{text} {context.LastDestination}";
        var entities = _entities.Extract(contextualText);
        if (intent is AIIntent.Greeting or AIIntent.Introduction or AIIntent.NameRecall or AIIntent.Gratitude or AIIntent.Acknowledgement or AIIntent.Farewell or AIIntent.Clarification or AIIntent.GeneralConversation)
        {
            var conversational = new AIResponse(_responses.Build(intent, entities, Array.Empty<TravelAiListing>(), question: text, context: context), intent, true, Array.Empty<TravelAiListing>(), request.ConversationKey);
            await _conversation.SaveAsync(request, conversational, cancellationToken);
            return conversational;
        }
        var knowledge = intent == AIIntent.DestinationComparison
            ? await SearchComparisonKnowledgeAsync(entities, text, cancellationToken)
            : await _knowledge.SearchAsync(text, cancellationToken);
        var location = await FindPublishedLocationAsync(text, cancellationToken);
        entities = _entities.WithLocation(entities, location);
        var capability = intent switch { AIIntent.DestinationSearch => "destination-search", AIIntent.HotelSearch => "hotel-search", AIIntent.RestaurantSearch => "restaurant-search", AIIntent.ResortSearch => "resort-search", AIIntent.TransportSearch => "transport-search", AIIntent.DestinationComparison => "destination-comparison", AIIntent.TripPlanning => "trip-planning", AIIntent.BudgetEstimation => "budget-estimation", AIIntent.BookingInquiry => "booking-inquiry", AIIntent.ContactInquiry => "contact-inquiry", AIIntent.BookingStatus => "booking-status", _ => null };
        if (capability != null && !await _capabilities.IsEnabledAsync(capability, cancellationToken))
        {
            var disabled = new AIResponse("That Karnel AI capability is currently disabled by the administrator. Please choose another travel request.", intent, false, Array.Empty<TravelAiListing>(), request.ConversationKey);
            await _conversation.SaveAsync(request, disabled, cancellationToken);
            return disabled;
        }
        var category = intent switch
        {
            AIIntent.HotelSearch or AIIntent.HotelPriceInquiry => "hotel",
            AIIntent.RestaurantSearch or AIIntent.RestaurantPriceInquiry => "restaurant",
            AIIntent.ResortSearch => "resort",
            AIIntent.TransportSearch or AIIntent.TransportFareInquiry => "transport",
            AIIntent.DestinationSearch or AIIntent.DestinationRecommendation => "destination",
            _ => null
        };
        AISearchResult search;
        if (intent is AIIntent.Greeting or AIIntent.Help or AIIntent.Unsupported or AIIntent.GeneralTravelInformation or AIIntent.DestinationInformation or AIIntent.DestinationComparison)
            search = new AISearchResult(Array.Empty<TravelAiListing>(), 0, 1, 10);
        else
        {
            var listingQuery = intent is AIIntent.DestinationSearch or AIIntent.HotelSearch or AIIntent.RestaurantSearch or AIIntent.ResortSearch or AIIntent.TransportSearch
                ? entities.Location ?? text
                : text;
            search = await _search.SearchAsync(new AISearchRequest(listingQuery, category, entities.Location, entities.MaximumBudget), cancellationToken);
        }
        if (intent == AIIntent.DestinationRecommendation && string.IsNullOrWhiteSpace(entities.Location))
            search = await _search.SearchAsync(new AISearchRequest(null, "destination", null, entities.MaximumBudget, 1, 10), cancellationToken);

        var plan = intent == AIIntent.TripPlanning && !string.IsNullOrWhiteSpace(entities.Location)
            ? await _planner.PlanAsync(entities, cancellationToken)
            : null;
        if (intent == AIIntent.TripPlanning && string.IsNullOrWhiteSpace(entities.Location))
        {
            var noDestination = new AIResponse("Please choose a destination first, or ask me to recommend one. I won't create a destination-less itinerary.", intent, true, search.Items, request.ConversationKey);
            await _conversation.SaveAsync(request, noDestination, cancellationToken);
            return noDestination;
        }
        var response = new AIResponse(_responses.Build(intent, entities, search.Items, plan, knowledge, text, context), intent, intent != AIIntent.Unsupported || knowledge.Count > 0, search.Items, request.ConversationKey);
        await _conversation.SaveAsync(request, response, cancellationToken);
        return response;
    }

    private async Task<string?> FindPublishedLocationAsync(string text, CancellationToken cancellationToken)
    {
        var candidates = new List<string>();
        candidates.AddRange(await _db.Destinations.AsNoTracking().Where(x => x.IsPublished).Select(x => x.Name).ToListAsync(cancellationToken));
        candidates.AddRange(await _db.Destinations.AsNoTracking().Where(x => x.IsPublished && x.Region != null).Select(x => x.Region!).ToListAsync(cancellationToken));
        candidates.AddRange(await _db.Hotels.AsNoTracking().Where(x => x.IsPublished && x.City != null).Select(x => x.City!).ToListAsync(cancellationToken));
        candidates.AddRange(await _db.Resorts.AsNoTracking().Where(x => x.IsPublished && x.CityOrRegion != null).Select(x => x.CityOrRegion!).ToListAsync(cancellationToken));
        candidates.AddRange(await _db.Restaurants.AsNoTracking().Where(x => x.IsPublished && x.City != null).Select(x => x.City!).ToListAsync(cancellationToken));
        candidates.AddRange(await _db.TransportServices.AsNoTracking().Where(x => x.IsPublished && x.StartingCity != null).Select(x => x.StartingCity!).ToListAsync(cancellationToken));
        candidates.AddRange(await _db.TransportServices.AsNoTracking().Where(x => x.IsPublished && x.DestinationCity != null).Select(x => x.DestinationCity!).ToListAsync(cancellationToken));

        return candidates
            .Where(x => x.Length >= 3 && text.Contains(x, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(x => x.Length)
            .FirstOrDefault();
    }

    private async Task<IReadOnlyList<AIKnowledgeMatch>> SearchComparisonKnowledgeAsync(AIEntities entities, string text, CancellationToken cancellationToken)
    {
        var names = entities.Destinations is { Count: > 0 } ? entities.Destinations : new[] { entities.DestinationA, entities.DestinationB }.Where(x => x != null).Select(x => x!).ToArray();
        if (names.Count == 0) return await _knowledge.SearchAsync(text, cancellationToken);
        var matches = new List<AIKnowledgeMatch>();
        foreach (var name in names)
            matches.AddRange(await _knowledge.SearchAsync(name, cancellationToken));
        return matches.GroupBy(x => x.Entry.Id == 0 ? $"title:{x.Entry.Title}" : $"id:{x.Entry.Id}").Select(x => x.OrderByDescending(m => m.Score).First()).ToList();
    }
}
