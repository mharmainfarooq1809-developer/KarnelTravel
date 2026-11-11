using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface ICustomerAccountService
{
    Task<CustomerAccount?> CreateAsync(string fullName, string email, string phoneNumber, string password, CancellationToken cancellationToken = default);
    Task<CustomerAccount?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    Task<string?> CreatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);
    Task InvalidatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(int customerId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<CustomerAccount?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int id, string fullName, string phoneNumber, CancellationToken cancellationToken = default);
}
