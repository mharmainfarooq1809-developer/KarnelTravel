using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _db;
    public AuditLogService(ApplicationDbContext db) => _db = db;

    public async Task RecordAsync(string? administrator, string action, string description, string? entityType = null, string? entityId = null, bool succeeded = true, CancellationToken cancellationToken = default)
    {
        _db.AdminAuditLogs.Add(new AdminAuditLog
        {
            Administrator = administrator,
            Action = action,
            Description = description,
            EntityType = entityType,
            EntityId = entityId,
            Succeeded = succeeded,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminAuditLog>> RecentAsync(string? search = null, string? action = null, CancellationToken cancellationToken = default)
    {
        var query = _db.AdminAuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Description.Contains(search) || (x.Administrator != null && x.Administrator.Contains(search)));
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action);
        return await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(cancellationToken);
    }
}
