using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public sealed class CustomerAccountService : ICustomerAccountService
{
    private static readonly TimeSpan ResetLifetime = TimeSpan.FromHours(1);
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<CustomerAccount> _hasher;

    public CustomerAccountService(ApplicationDbContext db, IPasswordHasher<CustomerAccount> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<CustomerAccount?> CreateAsync(string fullName, string email, string phoneNumber, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await _db.CustomerAccounts.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
            return null;

        var account = new CustomerAccount
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = phoneNumber.Trim()
        };
        account.PasswordHash = _hasher.HashPassword(account, password);
        _db.CustomerAccounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<CustomerAccount?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(x => x.Email == NormalizeEmail(email), cancellationToken);
        if (account is null || account.LockoutEndUtc > DateTime.UtcNow)
            return null;

        var result = _hasher.VerifyHashedPassword(account, account.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            account.FailedLoginAttempts++;
            if (account.FailedLoginAttempts >= 5)
            {
                account.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
                account.FailedLoginAttempts = 0;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        account.FailedLoginAttempts = 0;
        account.LockoutEndUtc = null;
        await _db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<string?> CreatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(
            x => x.Email == NormalizeEmail(email), cancellationToken);
        if (account is null)
            return null;

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        account.PasswordResetCodeHash = HashToken(code);
        account.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);
        account.PasswordResetTokenHash = null;
        account.PasswordResetTokenExpiresAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return code;
    }

    public async Task InvalidatePasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(
            x => x.Email == NormalizeEmail(email), cancellationToken);
        if (account is null)
            return;

        account.PasswordResetCodeHash = null;
        account.PasswordResetCodeExpiresAt = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(
            x => x.Email == NormalizeEmail(email) &&
                 x.PasswordResetCodeHash == HashToken(code.Trim()) &&
                 x.PasswordResetCodeExpiresAt > DateTime.UtcNow,
            cancellationToken);
        if (account is null)
            return false;

        account.PasswordHash = _hasher.HashPassword(account, newPassword);
        account.PasswordResetCodeHash = null;
        account.PasswordResetCodeExpiresAt = null;
        account.FailedLoginAttempts = 0;
        account.LockoutEndUtc = null;
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(x => x.Email == NormalizeEmail(email), cancellationToken);
        if (account is null)
            return null;

        var token = CreateToken();
        account.PasswordResetTokenHash = HashToken(token);
        account.PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(ResetLifetime);
        await _db.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(x =>
            x.PasswordResetTokenHash == HashToken(token) &&
            x.PasswordResetTokenExpiresAt > DateTime.UtcNow,
            cancellationToken);
        if (account is null)
            return false;

        account.PasswordHash = _hasher.HashPassword(account, newPassword);
        account.PasswordResetTokenHash = null;
        account.PasswordResetTokenExpiresAt = null;
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int customerId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FindAsync([customerId], cancellationToken);
        if (account is null || _hasher.VerifyHashedPassword(account, account.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
            return false;

        account.PasswordHash = _hasher.HashPassword(account, newPassword);
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<CustomerAccount?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.CustomerAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<CustomerAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _db.CustomerAccounts.FirstOrDefaultAsync(x => x.Email == NormalizeEmail(email), cancellationToken);

    public async Task<bool> UpdateProfileAsync(int id, string fullName, string phoneNumber, CancellationToken cancellationToken = default)
    {
        var account = await _db.CustomerAccounts.FindAsync([id], cancellationToken);
        if (account is null)
            return false;

        account.FullName = fullName.Trim();
        account.PhoneNumber = phoneNumber.Trim();
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string CreateCode() => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
