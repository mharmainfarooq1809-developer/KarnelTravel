using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface ISettingsService
{
    Task<ApplicationSettings> GetAsync(CancellationToken cancellationToken = default);
    Task<ApplicationSettings> SaveAsync(ApplicationSettings settings, string? administrator, CancellationToken cancellationToken = default);
}
