using KarnelTravelGuide.Models;

namespace KarnelTravelGuide.Services;

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string email, string password);
    Task<AdminUser?> FindByIdAsync(string id);
}