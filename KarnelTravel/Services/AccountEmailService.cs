using System.Net;
using System.Net.Mail;
using KarnelTravel.Models;

namespace KarnelTravel.Services;

public sealed class AccountEmailService : IAccountEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountEmailService> _logger;

    public AccountEmailService(IConfiguration configuration, ILogger<AccountEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetCodeAsync(CustomerAccount account, string code, CancellationToken cancellationToken = default)
    {
        var smtp = _configuration.GetSection("Email:Smtp");
        var host = smtp["Host"];
        var from = smtp["From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("Password reset email skipped for {Email}: SMTP is not configured.", account.Email);
            return;
        }

        try
        {
            using var message = new MailMessage(
                from,
                account.Email,
                "Your Karnel Travel Guide password reset code",
                $"Hello {account.FullName},\n\nYour password reset code is: {code}\n\nThis code expires in 15 minutes.\n\nIf you did not request this, you can ignore this email.");
            using var client = new SmtpClient(host, smtp.GetValue("Port", 587))
            {
                EnableSsl = smtp.GetValue("EnableSsl", true),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            var username = smtp["Username"];
            if (!string.IsNullOrWhiteSpace(username))
                client.Credentials = new NetworkCredential(username, smtp["Password"]);

            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Password reset email failed for {Email}.", account.Email);
    }
    }
}
