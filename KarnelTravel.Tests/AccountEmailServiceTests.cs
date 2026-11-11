using KarnelTravel.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace KarnelTravel.Tests;

public sealed class AccountEmailServiceTests
{
    [Fact]
    public async Task Missing_smtp_credentials_are_handled_without_sending()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:Smtp:Host"] = "smtp.example.test",
                ["Email:Smtp:From"] = "sender@example.test",
                ["Email:Smtp:Username"] = "",
                ["Email:Smtp:Password"] = ""
            })
            .Build();

        var service = new AccountEmailService(
            configuration,
            NullLogger<AccountEmailService>.Instance);

        var sent = await service.SendPasswordResetCodeAsync(
            "recipient@example.test",
            "Recipient",
            "123456");

        Assert.False(sent);
    }
}