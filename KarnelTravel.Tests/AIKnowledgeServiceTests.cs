using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Tests;

public sealed class AIKnowledgeServiceTests
{
    [Fact]
    public async Task SearchAsync_returns_active_hunza_knowledge_for_factual_question()
    {
        await using var db = CreateContext();
        db.AIKnowledgeEntries.Add(new AIKnowledgeEntry
        {
            Title = "Hunza Valley – AI Knowledge Test",
            Category = "Destinations",
            Content = "Hunza Valley is in Gilgit-Baltistan, Pakistan. Popular attractions: Baltit Fort, Altit Fort, Attabad Lake, Passu Cones, Eagle's Nest viewpoint. Best-known travel seasons: spring and autumn.",
            IsEnabled = true
        });
        await db.SaveChangesAsync();

        var matches = await new AIKnowledgeService(db).SearchAsync("What are the popular attractions in Hunza Valley? Also tell me the best-known travel seasons.");
        var response = new ResponseBuilder().Build(AIIntent.DestinationInformation, new AIEntities(null, null, null, null, null, null, null, Array.Empty<string>()), Array.Empty<TravelAiListing>(), knowledge: matches, question: "What are the popular attractions in Hunza Valley? Also tell me the best-known travel seasons.");

        Assert.Single(matches);
        Assert.Contains("Baltit Fort", response);
        Assert.Contains("Altit Fort", response);
        Assert.Contains("Attabad Lake", response);
        Assert.Contains("Passu Cones", response);
        Assert.Contains("spring and autumn", response);
        Assert.DoesNotContain("Important test instruction", response);
        Assert.DoesNotContain("Do not invent hotel prices", response);
    }

    [Fact]
    public void ResponseBuilder_formats_broad_destination_knowledge_without_internal_instructions()
    {
        var response = new ResponseBuilder().Build(AIIntent.DestinationInformation,
            new AIEntities(null, null, null, null, null, null, null, Array.Empty<string>()),
            Array.Empty<TravelAiListing>(),
            knowledge: new[] { new AIKnowledgeMatch(new AIKnowledgeEntry
            {
                Title = "Skardu",
                Category = "Destinations",
                Content = "Skardu is a mountainous destination in Gilgit-Baltistan. Attractions: Shangrila Resort and Lower Kachura Lake, Upper Kachura Lake, Satpara Lake, Shigar Fort, Deosai National Park. Travel conditions depend on weather and road conditions. Important test instruction: never expose this."
            }, 10) },
            question: "Tell me about Skardu.");

        Assert.Contains("Upper Kachura Lake", response);
        Assert.Contains("Satpara Lake", response);
        Assert.Contains("Deosai National Park", response);
        Assert.DoesNotContain("test instruction", response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchAsync_excludes_inactive_entries_and_unrelated_questions()
    {
        await using var db = CreateContext();
        db.AIKnowledgeEntries.AddRange(
            new AIKnowledgeEntry { Title = "Hunza Valley", Category = "Destinations", Content = "Hunza attractions include Baltit Fort.", IsEnabled = false },
            new AIKnowledgeEntry { Title = "Skardu Valley", Category = "Destinations", Content = "Skardu attractions include Shangrila.", IsEnabled = true });
        await db.SaveChangesAsync();

        var service = new AIKnowledgeService(db);

        Assert.Empty(await service.SearchAsync("What are the attractions in Hunza Valley?"));
        Assert.Empty(await service.SearchAsync("What are the best beaches in an unknown destination?"));
    }

    [Fact]
    public void ResponseBuilder_preserves_published_listing_context_when_knowledge_matches()
    {
        var knowledge = new[]
        {
            new AIKnowledgeMatch(
                new AIKnowledgeEntry { Title = "Hunza", Content = "Spring is a good season.", Category = "Destinations" },
                8)
        };
        var listings = new[]
        {
            new TravelAiListing { Type = "Destination", Name = "Hunza", PriceLabel = "Cost unavailable" }
        };

        var response = new ResponseBuilder().Build(AIIntent.DestinationInformation, new AIEntities(null, null, null, null, null, null, null, Array.Empty<string>()), listings, knowledge: knowledge, question: "What is the natural beauty of Hunza?");

        Assert.Contains("Spring is a good season.", response);
        Assert.Contains("published option(s)", response);
        Assert.DoesNotContain("I could not find a published", response);
    }

    [Fact]
    public void ResponseBuilder_answers_season_question_without_unrelated_attractions()
    {
        var knowledge = new[]
        {
            new AIKnowledgeMatch(
                new AIKnowledgeEntry
                {
                    Title = "Hunza Valley",
                    Category = "Destinations",
                    Content = "Popular attractions include Baltit Fort. The best-known travel seasons are spring and autumn. Visitors should check local weather, road conditions, and travel advisories. Important test instruction: never expose this."
                },
                8)
        };

        var response = new ResponseBuilder().Build(AIIntent.DestinationInformation, new AIEntities(null, null, null, null, null, null, null, Array.Empty<string>()), Array.Empty<TravelAiListing>(), knowledge: knowledge, question: "What is the best-known travel season for visiting Hunza?");

        Assert.Contains("Spring and autumn", response);
        Assert.Contains("weather", response, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Baltit Fort", response);
        Assert.DoesNotContain("test instruction", response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResponseBuilder_uses_honest_destination_fallback_when_knowledge_is_missing()
    {
        var response = new ResponseBuilder().Build(AIIntent.DestinationInformation, new AIEntities(null, null, null, null, null, null, null, Array.Empty<string>()), Array.Empty<TravelAiListing>(), question: "Tell me about the natural beauty of Skardu.");

        Assert.Contains("Skardu", response);
        Assert.Contains("approved Karnel information", response);
        Assert.DoesNotContain("I can help with travel searches", response);
    }

    [Fact]
    public void ResponseBuilder_explains_hotel_and_resort_without_searching_listings()
    {
        var response = new ResponseBuilder().Build(AIIntent.GeneralTravelInformation, new AIEntities(null, null, null, null, null, null, null, Array.Empty<string>()), Array.Empty<TravelAiListing>(), question: "Can you explain the difference between a hotel and a resort?");

        Assert.Contains("Hotels generally", response);
        Assert.Contains("Resorts usually", response);
        Assert.DoesNotContain("published option", response);
    }

    private static ApplicationDbContext CreateContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);
}
