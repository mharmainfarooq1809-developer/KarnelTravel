using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _db;

    public SettingsService(ApplicationDbContext db) => _db = db;

    public async Task<ApplicationSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? new ApplicationSettings();
    }

    public async Task<ApplicationSettings> SaveAsync(ApplicationSettings settings, string? administrator, CancellationToken cancellationToken = default)
    {
        var stored = await _db.ApplicationSettings.FirstOrDefaultAsync(cancellationToken);
        if (stored is null)
        {
            settings.Id = 0;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = administrator;
            _db.ApplicationSettings.Add(settings);
        }
        else
        {
            _db.Entry(stored).CurrentValues.SetValues(settings);
            stored.UpdatedAt = DateTime.UtcNow;
            stored.UpdatedBy = administrator;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return stored ?? settings;
    }
}
