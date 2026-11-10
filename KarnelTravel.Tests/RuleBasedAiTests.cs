using KarnelTravel.Services.AI;

namespace KarnelTravel.Tests;

public sealed class RuleBasedAiTests
{
    [Theory]
    [InlineData("hello", AIIntent.Greeting)]
    [InlineData("Hi, my name is Harman", AIIntent.Introduction)]
    [InlineData("What is my name?", AIIntent.NameRecall)]
    [InlineData("Thank you", AIIntent.Gratitude)]
    [InlineData("Okay", AIIntent.Acknowledgement)]
    [InlineData("Goodbye", AIIntent.Farewell)]
    [InlineData("Can you clarify?", AIIntent.Clarification)]
    [InlineData("find a hotel", AIIntent.HotelSearch)]
    [InlineData("show restaurants", AIIntent.RestaurantSearch)]
    [InlineData("plan a 4 day trip", AIIntent.TripPlanning)]
    [InlineData("compare destinations", AIIntent.DestinationComparison)]
    [InlineData("Can you explain the difference between a hotel and a resort?", AIIntent.GeneralTravelInformation)]
    [InlineData("Tell me about the natural beauty of Skardu.", AIIntent.DestinationInformation)]
    [InlineData("I want to know about Hunza Valley.", AIIntent.DestinationInformation)]
    [InlineData("I have three days. Suggest a destination.", AIIntent.DestinationRecommendation)]
    [InlineData("Suggest a three-day trip to Hunza.", AIIntent.TripPlanning)]
    [InlineData("What is the hotel price in Hunza?", AIIntent.HotelPriceInquiry)]
    [InlineData("What is the transport fare to Skardu?", AIIntent.TransportFareInquiry)]
    [InlineData("Compare Hunza and Skardu.", AIIntent.DestinationComparison)]
    [InlineData("Show me hotels in Lahore.", AIIntent.HotelSearch)]
    [InlineData("Find resorts in Hunza.", AIIntent.ResortSearch)]
    [InlineData("What should travellers check before visiting northern Pakistan?", AIIntent.GeneralTravelInformation)]
    [InlineData("tell me a joke", AIIntent.Unsupported)]
    public void IntentDetector_classifies_supported_and_unsupported_requests(string message, AIIntent expected)
    {
        Assert.Equal(expected, new IntentDetector().Detect(message));
    }

    [Fact]
    public void EntityExtractor_reads_budget_duration_travellers_and_preference()
    {
        var result = new EntityExtractor().Extract("family trip for 5 days, 4 travelers, budget under PKR 120,000");
        Assert.Equal(120000, result.MaximumBudget);
        Assert.Equal(5, result.DurationDays);
        Assert.Equal(4, result.Travelers);
        Assert.Equal("family", result.Preference);
    }

    [Fact]
    public void EntityExtractor_keeps_recommendation_without_destination()
    {
        var result = new EntityExtractor().Extract("I have three days. Suggest a destination.");
        Assert.Null(result.Location);
        Assert.Equal(3, result.DurationDays);
    }

    [Fact]
    public void EntityExtractor_extracts_two_comparison_destinations()
    {
        var result = new EntityExtractor().Extract("Compare Hunza and Skardu.");
        Assert.Equal("Hunza", result.DestinationA);
        Assert.Equal("Skardu", result.DestinationB);
        Assert.Equal(new[] { "Hunza", "Skardu" }, result.Destinations);
    }

    [Fact]
    public void Natural_conversation_does_not_use_travel_fallback_intents()
    {
        var detector = new IntentDetector();

        Assert.NotEqual(AIIntent.Unsupported, detector.Detect("Hi, my name is Harman"));
        Assert.NotEqual(AIIntent.Unsupported, detector.Detect("Thank you"));
    }

    [Fact]
    public void Published_destination_reply_is_selected_when_previous_turn_requests_choice()
    {
        var context = new AIConversationContext(null, null, "Based on published destinations, choose a destination.", AIIntent.DestinationRecommendation);

        Assert.True(ContextualFollowUpDetector.IsDestinationSelection("Skardu", context, "Skardu"));
    }

    [Fact]
    public void Unrelated_follow_up_does_not_override_current_intent()
    {
        var context = new AIConversationContext(null, null, "Based on published destinations, choose a destination.", AIIntent.DestinationRecommendation);

        Assert.False(ContextualFollowUpDetector.IsDestinationSelection("What is the weather?", context, null));
    }
}
