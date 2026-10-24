using KarnelTravel.Models;
using KarnelTravel.Models;

namespace KarnelTravel.Services; // <-- Changed from KarnelTravel

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string email, string password);
    Task<AdminUser?> FindByIdAsync(string id);
    Task<AdminUser?> RegisterUserAsync(string email, string name, string password);
}