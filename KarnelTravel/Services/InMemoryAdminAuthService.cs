using KarnelTravel.Models;
using Microsoft.AspNetCore.Identity;

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
}