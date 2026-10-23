using KarnelTravelGuide.Models;
using Microsoft.AspNetCore.Identity;

namespace KarnelTravelGuide.Services;

/// <summary>
/// Seeded, in-memory implementation for the design phase.
/// Passwords are hashed with ASP.NET Core Identity's PasswordHasher,
/// which is PBKDF2 with a per-user salt — same guarantees as production.
/// Swap this class for an EF Core implementation backed by SQL Server
/// when the data layer is added.
/// </summary>
public class InMemoryAdminAuthService : IAdminAuthService
{
    private readonly List<AdminUser> _users = new();
    private readonly IPasswordHasher<AdminUser> _hasher;

    public InMemoryAdminAuthService(IPasswordHasher<AdminUser> hasher)
    {
        _hasher = hasher;
        Seed();
    }

    private void Seed()
    {
        Add("u-amara", "amara@karneltravel.co.za", "Amara Nkosi", "Karnel!2025");
        Add("u-dev", "admin@karnel.local", "Dev Admin", "Admin123!");
    }

    private void Add(string id, string email, string name, string plainPassword)
    {
        var user = new AdminUser
        {
            Id = id,
            Email = email,
            DisplayName = name,
            Role = "Admin"
        };
        user.PasswordHash = _hasher.HashPassword(user, plainPassword);
        _users.Add(user);
    }

    public Task<AdminUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null)
            return Task.FromResult<AdminUser?>(null);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return Task.FromResult<AdminUser?>(
            result is PasswordVerificationResult.Success
                   or PasswordVerificationResult.SuccessRehashNeeded
                ? user
                : null);
    }

    public Task<AdminUser?> FindByIdAsync(string id)
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
}