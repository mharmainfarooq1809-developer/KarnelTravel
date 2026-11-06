using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services.AI;

public sealed record AIDashboardData(
    int Conversations,
    int Messages,
    int SupportedMessages,
    int UnsupportedMessages,
    int FeedbackCount,
    int HelpfulFeedback,
    int EnabledCapabilities,
    IReadOnlyList<(string Intent, int Count)> PopularIntents,
    IReadOnlyList<AIConversation> RecentConversations);

public interface IAIAdministrationService
{
    Task<AIConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task SaveConfigurationAsync(AIConfiguration configuration, string? administrator, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AICapability>> GetCapabilitiesAsync(CancellationToken cancellationToken = default);
    Task SetCapabilityAsync(string key, bool enabled, string? administrator, CancellationToken cancellationToken = default);
    Task<AIRecommendationSettings> GetRecommendationSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveRecommendationSettingsAsync(AIRecommendationSettings settings, string? administrator, CancellationToken cancellationToken = default);
    Task<AIDashboardData> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIKnowledgeEntry>> GetKnowledgeAsync(string? search, CancellationToken cancellationToken = default);
    Task<AIKnowledgeEntry?> GetKnowledgeEntryAsync(int id, CancellationToken cancellationToken = default);
    Task SaveKnowledgeEntryAsync(AIKnowledgeEntry entry, string? administrator, CancellationToken cancellationToken = default);
    Task DeleteKnowledgeEntryAsync(int id, string? administrator, CancellationToken cancellationToken = default);
    Task SetKnowledgeEntryEnabledAsync(int id, bool enabled, string? administrator, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIQuickPrompt>> GetPromptsAsync(CancellationToken cancellationToken = default);
    Task<AIQuickPrompt?> GetPromptAsync(int id, CancellationToken cancellationToken = default);
    Task SavePromptAsync(AIQuickPrompt prompt, string? administrator, CancellationToken cancellationToken = default);
    Task DeletePromptAsync(int id, string? administrator, CancellationToken cancellationToken = default);
    Task SetPromptEnabledAsync(int id, bool enabled, string? administrator, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIConversation>> GetConversationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIFeedback>> GetFeedbackAsync(bool? helpful, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIAuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
}

public sealed class AIAdministrationService : IAIAdministrationService, IAICapabilityService
{
    private static readonly (string Key, string Name)[] DefaultCapabilities =
    {
        ("destination-search", "Destination search"), ("hotel-search", "Hotel search"),
        ("restaurant-search", "Restaurant search"), ("resort-search", "Resort search"),
        ("transport-search", "Transport search"), ("destination-comparison", "Destination comparison"),
        ("trip-planning", "Trip planning"), ("budget-estimation", "Budget estimation"),
        ("booking-inquiry", "Booking inquiry"), ("contact-inquiry", "Contact inquiry"),
        ("booking-status", "Booking status retrieval")
    };

    private readonly ApplicationDbContext _db;
    public AIAdministrationService(ApplicationDbContext db) => _db = db;

    public async Task<AIConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var result = await _db.AIConfigurations.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (result != null) return result;
        result = new AIConfiguration();
        _db.AIConfigurations.Add(result);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default)
    {
        var configuration = await GetConfigurationAsync(cancellationToken);
        if (!configuration.IsEnabled) return false;
        var capability = await _db.AICapabilities.AsNoTracking().SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
        return capability?.IsEnabled ?? false;
    }

    public async Task SaveConfigurationAsync(AIConfiguration configuration, string? administrator, CancellationToken cancellationToken = default)
    {
        var current = await GetConfigurationAsync(cancellationToken);
        current.IsEnabled = configuration.IsEnabled;
        current.AssistantName = configuration.AssistantName.Trim();
        current.WelcomeMessage = configuration.WelcomeMessage.Trim();
        current.ResponseTone = configuration.ResponseTone.Trim();
        current.MaximumMessageLength = Math.Clamp(configuration.MaximumMessageLength, 100, 10000);
        current.ConversationMessageLimit = Math.Clamp(configuration.ConversationMessageLimit, 1, 100);
        current.SystemInstructions = configuration.SystemInstructions?.Trim() ?? string.Empty;
        current.UpdatedAt = DateTime.UtcNow;
        current.UpdatedBy = administrator;
        await WriteAuditAsync(administrator, "Configuration updated", true, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AICapability>> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _db.AICapabilities.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var changed = false;
        foreach (var item in DefaultCapabilities)
        {
            if (existing.Any(x => x.Key == item.Key)) continue;
            var capability = new AICapability { Key = item.Key, Name = item.Name };
            _db.AICapabilities.Add(capability);
            existing.Add(capability);
            changed = true;
        }
        if (changed) await _db.SaveChangesAsync(cancellationToken);
        return existing.OrderBy(x => x.Name).ToList();
    }

    public async Task SetCapabilityAsync(string key, bool enabled, string? administrator, CancellationToken cancellationToken = default)
    {
        var capability = await _db.AICapabilities.SingleOrDefaultAsync(x => x.Key == key, cancellationToken) ?? throw new KeyNotFoundException("Capability was not found.");
        capability.IsEnabled = enabled;
        capability.UpdatedAt = DateTime.UtcNow;
        await WriteAuditAsync(administrator, $"Capability {key} set to {enabled}", true, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AIRecommendationSettings> GetRecommendationSettingsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _db.Set<AIRecommendationSettings>().FirstOrDefaultAsync(cancellationToken);
        if (result != null) return result;
        result = new AIRecommendationSettings();
        _db.Set<AIRecommendationSettings>().Add(result);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task SaveRecommendationSettingsAsync(AIRecommendationSettings settings, string? administrator, CancellationToken cancellationToken = default)
    {
        if (settings.LocationWeight + settings.BudgetWeight + settings.FeaturedWeight + settings.CategoryWeight <= 0) throw new ArgumentException("At least one recommendation weight must be greater than zero.");
        var current = await GetRecommendationSettingsAsync(cancellationToken);
        current.LocationWeight = settings.LocationWeight; current.BudgetWeight = settings.BudgetWeight; current.FeaturedWeight = settings.FeaturedWeight; current.CategoryWeight = settings.CategoryWeight;
        current.PreferFeatured = settings.PreferFeatured; current.MatchBudget = settings.MatchBudget; current.MatchDestination = settings.MatchDestination; current.UpdatedAt = DateTime.UtcNow; current.UpdatedBy = administrator;
        await WriteAuditAsync(administrator, "Recommendation settings updated", true, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AIDashboardData> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var messages = _db.AIMessages.AsNoTracking();
        var feedback = _db.AIFeedback.AsNoTracking();
        var popular = await messages.Where(x => x.Intent != null).GroupBy(x => x.Intent!).Select(x => new { Intent = x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).Take(10).ToListAsync(cancellationToken);
        return new AIDashboardData(await _db.AIConversations.CountAsync(cancellationToken), await messages.CountAsync(cancellationToken), await messages.CountAsync(x => x.WasSupported, cancellationToken), await messages.CountAsync(x => !x.WasSupported, cancellationToken), await feedback.CountAsync(cancellationToken), await feedback.CountAsync(x => x.IsHelpful, cancellationToken), await _db.AICapabilities.CountAsync(x => x.IsEnabled, cancellationToken), popular.Select(x => (x.Intent, x.Count)).ToList(), await _db.AIConversations.AsNoTracking().OrderByDescending(x => x.UpdatedAt).Take(10).ToListAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<AIKnowledgeEntry>> GetKnowledgeAsync(string? search, CancellationToken cancellationToken = default) => await _db.AIKnowledgeEntries.AsNoTracking().Where(x => string.IsNullOrWhiteSpace(search) || x.Title.Contains(search!) || x.Content.Contains(search!)).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
    public Task<AIKnowledgeEntry?> GetKnowledgeEntryAsync(int id, CancellationToken cancellationToken = default) => _db.AIKnowledgeEntries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task SaveKnowledgeEntryAsync(AIKnowledgeEntry entry, string? administrator, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entry.Title) || string.IsNullOrWhiteSpace(entry.Content) || string.IsNullOrWhiteSpace(entry.Category)) throw new ArgumentException("Title, content, and category are required.");
        var current = entry.Id == 0 ? new AIKnowledgeEntry() : await _db.AIKnowledgeEntries.FirstOrDefaultAsync(x => x.Id == entry.Id, cancellationToken) ?? throw new KeyNotFoundException("Knowledge entry was not found.");
        current.Title = entry.Title.Trim(); current.Content = entry.Content.Trim(); current.Category = entry.Category.Trim(); current.IsEnabled = entry.IsEnabled; current.UpdatedAt = DateTime.UtcNow;
        if (entry.Id == 0) _db.AIKnowledgeEntries.Add(current);
        await WriteAuditAsync(administrator, entry.Id == 0 ? "Knowledge entry created" : "Knowledge entry updated", true, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteKnowledgeEntryAsync(int id, string? administrator, CancellationToken cancellationToken = default)
    {
        var entry = await _db.AIKnowledgeEntries.FindAsync(new object[] { id }, cancellationToken) ?? throw new KeyNotFoundException("Knowledge entry was not found.");
        _db.AIKnowledgeEntries.Remove(entry); await WriteAuditAsync(administrator, "Knowledge entry deleted", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task SetKnowledgeEntryEnabledAsync(int id, bool enabled, string? administrator, CancellationToken cancellationToken = default)
    {
        var entry = await _db.AIKnowledgeEntries.FindAsync(new object[] { id }, cancellationToken) ?? throw new KeyNotFoundException("Knowledge entry was not found.");
        entry.IsEnabled = enabled; entry.UpdatedAt = DateTime.UtcNow; await WriteAuditAsync(administrator, "Knowledge entry status changed", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<AIQuickPrompt>> GetPromptsAsync(CancellationToken cancellationToken = default) => await _db.AIQuickPrompts.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToListAsync(cancellationToken);
    public Task<AIQuickPrompt?> GetPromptAsync(int id, CancellationToken cancellationToken = default) => _db.AIQuickPrompts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task SavePromptAsync(AIQuickPrompt prompt, string? administrator, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt.Label) || string.IsNullOrWhiteSpace(prompt.Prompt) || prompt.DisplayOrder < 0) throw new ArgumentException("Label, prompt text, and a non-negative order are required.");
        var current = prompt.Id == 0 ? new AIQuickPrompt() : await _db.AIQuickPrompts.FirstOrDefaultAsync(x => x.Id == prompt.Id, cancellationToken) ?? throw new KeyNotFoundException("Quick prompt was not found.");
        current.Label = prompt.Label.Trim(); current.Prompt = prompt.Prompt.Trim(); current.DisplayOrder = prompt.DisplayOrder; current.IsEnabled = prompt.IsEnabled;
        if (prompt.Id == 0) _db.AIQuickPrompts.Add(current);
        await WriteAuditAsync(administrator, prompt.Id == 0 ? "Quick prompt created" : "Quick prompt updated", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task DeletePromptAsync(int id, string? administrator, CancellationToken cancellationToken = default)
    {
        var prompt = await _db.AIQuickPrompts.FindAsync(new object[] { id }, cancellationToken) ?? throw new KeyNotFoundException("Quick prompt was not found.");
        _db.AIQuickPrompts.Remove(prompt); await WriteAuditAsync(administrator, "Quick prompt deleted", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task SetPromptEnabledAsync(int id, bool enabled, string? administrator, CancellationToken cancellationToken = default)
    {
        var prompt = await _db.AIQuickPrompts.FindAsync(new object[] { id }, cancellationToken) ?? throw new KeyNotFoundException("Quick prompt was not found.");
        prompt.IsEnabled = enabled; await WriteAuditAsync(administrator, "Quick prompt status changed", true, cancellationToken); await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<AIConversation>> GetConversationsAsync(CancellationToken cancellationToken = default) => await _db.AIConversations.AsNoTracking().OrderByDescending(x => x.UpdatedAt).Take(200).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<AIFeedback>> GetFeedbackAsync(bool? helpful, CancellationToken cancellationToken = default) => await _db.AIFeedback.AsNoTracking().Where(x => !helpful.HasValue || x.IsHelpful == helpful.Value).OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<AIAuditLog>> GetAuditLogsAsync(CancellationToken cancellationToken = default) => await _db.AIAuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(cancellationToken);

    private async Task WriteAuditAsync(string? administrator, string action, bool succeeded, CancellationToken cancellationToken) => _db.AIAuditLogs.Add(new AIAuditLog { Administrator = administrator, Action = action, Description = action, Succeeded = succeeded });
}
