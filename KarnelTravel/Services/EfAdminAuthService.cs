using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public class EfAdminAuthService : IAdminAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<AdminUser> _hasher;

    public EfAdminAuthService(ApplicationDbContext db, IPasswordHasher<AdminUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<AdminUser?> ValidateCredentialsAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user is null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    public async Task<AdminUser?> FindByIdAsync(string id)
    {
        if (!int.TryParse(id, out var intId)) return null;
        return await _db.AdminUsers.FindAsync(intId);
    }

    public async Task<AdminUser?> RegisterUserAsync(string email, string name, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await _db.AdminUsers.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (exists) return null;

        var user = new AdminUser
        {
            Email = normalizedEmail,
            DisplayName = name.Trim(),
            Role = "User"
        };

        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.AdminUsers.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.AdminUsers.FindAsync(userId);
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
            return false;

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        await _db.SaveChangesAsync();
        return true;
    }
}