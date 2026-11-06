using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIRecommendationSettings
{
    public int Id { get; set; }
    [Range(0, 100)] public decimal LocationWeight { get; set; } = 40;
    [Range(0, 100)] public decimal BudgetWeight { get; set; } = 25;
    [Range(0, 100)] public decimal FeaturedWeight { get; set; } = 15;
    [Range(0, 100)] public decimal CategoryWeight { get; set; } = 20;
    public bool PreferFeatured { get; set; } = true;
    public bool MatchBudget { get; set; } = true;
    public bool MatchDestination { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [StringLength(256)] public string? UpdatedBy { get; set; }
}
