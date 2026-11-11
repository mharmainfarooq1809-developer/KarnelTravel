
using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string email, string password);
    Task<AdminUser?> FindByIdAsync(string id);
    Task<AdminUser?> RegisterUserAsync(string email, string name, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<string?> CreatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);
    Task InvalidatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
}
