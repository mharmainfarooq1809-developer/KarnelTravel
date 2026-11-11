using KarnelTravel.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace KarnelTravel.Services;

public class InMemoryAdminAuthService : IAdminAuthService
{
    private readonly List<AdminUser> _users = new();
    private readonly IPasswordHasher<AdminUser> _hasher;
    private int _nextId = 3; // Starts at 3 because we seed 1 and 2

    public InMemoryAdminAuthService(IPasswordHasher<AdminUser> hasher)
    {
        _hasher = hasher;
        Seed();
    }

    private void Seed()
    {
        Add(1, "amara@karneltravel.co.za", "Amara Nkosi", "Karnel!2025");
        Add(2, "admin@karnel.local", "Dev Admin", "Admin123!");
    }

    private void Add(int id, string email, string name, string plainPassword)
    {
        var user = new AdminUser
        {
            Id = id, // Uses int now
            Email = email,
            DisplayName = name,
            Role = "Admin"
        };
        user.PasswordHash = _hasher.HashPassword(user, plainPassword);
        _users.Add(user);
    }

    public Task<AdminUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user is null) return Task.FromResult<AdminUser?>(null);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return Task.FromResult<AdminUser?>(
            result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded ? user : null);
    }

    public Task<AdminUser?> FindByIdAsync(string id)
    {
        if (!int.TryParse(id, out var intId)) return Task.FromResult<AdminUser?>(null);
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == intId));
    }

    public Task<AdminUser?> RegisterUserAsync(string email, string name, string password)
    {
        if (_users.Any(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult<AdminUser?>(null);

        var user = new AdminUser
        {
            Id = _nextId++,
            Email = email.Trim(),
            DisplayName = name.Trim(),
            Role = "User"
        };
        user.PasswordHash = _hasher.HashPassword(user, password);
        _users.Add(user);
        return Task.FromResult<AdminUser?>(user);
    }

    public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) ||
            string.IsNullOrWhiteSpace(newPassword) ||
            newPassword.Length < 6)
        {
            return Task.FromResult(false);
        }

        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user is null)
            return Task.FromResult(false);

        var verification = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            currentPassword);

        if (verification is not PasswordVerificationResult.Success and
            not PasswordVerificationResult.SuccessRehashNeeded)
        {
            return Task.FromResult(false);
        }

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        return Task.FromResult(true);
    }

    public Task<string?> CreatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user is null)
            return Task.FromResult<string?>(null);

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        user.PasswordResetCodeHash = HashToken(code);
        user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);
        return Task.FromResult<string?>(code);
    }

    public Task InvalidatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user is not null)
        {
            user.PasswordResetCodeHash = null;
            user.PasswordResetCodeExpiresAt = null;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) &&
            u.PasswordResetCodeHash == HashToken(code.Trim()) &&
            u.PasswordResetCodeExpiresAt > DateTime.UtcNow);
        if (user is null)
            return Task.FromResult(false);

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        user.PasswordResetCodeHash = null;
        user.PasswordResetCodeExpiresAt = null;
        return Task.FromResult(true);
    }

    private static string HashToken(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}