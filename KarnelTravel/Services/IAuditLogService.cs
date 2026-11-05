using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IAuditLogService
{
    Task RecordAsync(string? administrator, string action, string description, string? entityType = null, string? entityId = null, bool succeeded = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminAuditLog>> RecentAsync(string? search = null, string? action = null, CancellationToken cancellationToken = default);
}
