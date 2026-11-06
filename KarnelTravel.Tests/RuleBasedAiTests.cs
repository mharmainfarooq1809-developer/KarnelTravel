using KarnelTravel.Services.AI;

namespace KarnelTravel.Tests;

public sealed class RuleBasedAiTests
{
    [Theory]
    [InlineData("hello", AIIntent.Greeting)]
    [InlineData("find a hotel", AIIntent.HotelSearch)]
    [InlineData("show restaurants", AIIntent.RestaurantSearch)]
    [InlineData("plan a 4 day trip", AIIntent.TripPlanning)]
    [InlineData("compare destinations", AIIntent.DestinationComparison)]
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
}
