using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public class SettingsViewModel
{
    public ApplicationSettings Settings { get; set; } = new();
    public IReadOnlyList<AdminAuditLog> AuditLogs { get; set; } = Array.Empty<AdminAuditLog>();
    public bool EmailDeliveryConfigured { get; set; }
    public string? Search { get; set; }
    public string? ActionFilter { get; set; }
}
