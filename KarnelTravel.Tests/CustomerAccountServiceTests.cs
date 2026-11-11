using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Tests;

public sealed class CustomerAccountServiceTests
{
    [Fact]
    public async Task Registration_supports_multiple_independent_accounts_and_normalized_duplicates()
    {
        await using var db = CreateDatabase();
        var service = CreateService(db);

        var first = await service.CreateAsync("First Customer", "First@example.com", "+1 111", "Password123!");
        var second = await service.CreateAsync("Second Customer", "second@example.com", "+1 222", "Password123!");
        var duplicate = await service.CreateAsync("Duplicate", " FIRST@EXAMPLE.COM ", "+1 333", "Password123!");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Null(duplicate);
        Assert.Equal(2, await db.CustomerAccounts.CountAsync());
    }

    [Fact]
    public async Task Password_reset_code_is_single_use_and_updates_hash()
    {
        await using var db = CreateDatabase();
        var service = CreateService(db);
        var account = await service.CreateAsync("Customer", "customer@example.com", "+1 111", "OldPassword123!");
        Assert.NotNull(account);

        var resetCode = await service.CreatePasswordResetCodeAsync(account.Email);
        Assert.NotNull(resetCode);
        Assert.True(await service.ResetPasswordWithCodeAsync(account.Email, resetCode!, "NewPassword123!"));
        Assert.False(await service.ResetPasswordWithCodeAsync(account.Email, resetCode!, "AnotherPassword123!"));
        Assert.NotNull(await service.ValidateCredentialsAsync(account.Email, "NewPassword123!"));
        Assert.Null(await service.ValidateCredentialsAsync(account.Email, "OldPassword123!"));
    }

    [Fact]
    public async Task Password_reset_code_can_be_invalidated_when_delivery_fails()
    {
        await using var db = CreateDatabase();
        var service = CreateService(db);
        var account = await service.CreateAsync("Customer", "customer@example.com", "+1 111", "OldPassword123!");
        Assert.NotNull(account);

        var resetCode = await service.CreatePasswordResetCodeAsync(account.Email);
        Assert.NotNull(resetCode);

        await service.InvalidatePasswordResetCodeAsync(account.Email);

        Assert.False(await service.ResetPasswordWithCodeAsync(account.Email, resetCode!, "NewPassword123!"));
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static CustomerAccountService CreateService(ApplicationDbContext db) =>
        new(db, new PasswordHasher<CustomerAccount>());
}
