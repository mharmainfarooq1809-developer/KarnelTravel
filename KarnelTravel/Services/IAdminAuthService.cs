
using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string email, string password);
    Task<AdminUser?> FindByIdAsync(string id);
    Task<AdminUser?> RegisterUserAsync(string email, string name, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
