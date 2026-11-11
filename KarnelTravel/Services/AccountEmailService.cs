using System.Net;
using System.Net.Mail;

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

    public async Task<bool> SendPasswordResetCodeAsync(string email, string recipientName, string code, CancellationToken cancellationToken = default)
    {
        var smtp = _configuration.GetSection("Email:Smtp");
        var host = smtp["Host"];
        var username = smtp["Username"];
        var password = smtp["Password"];
        var from = smtp["From"];
        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(from) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Password reset email skipped for {Email}: SMTP is not configured.", email);
            return false;
        }

        try
        {
            using var message = new MailMessage(
                from,
                email,
                "Your Karnel Travel Guide password reset code",
                $"Hello {recipientName},\n\nYour password reset code is: {code}\n\nThis code expires in 15 minutes.\n\nIf you did not request this, you can ignore this email.");
            using var client = new SmtpClient(host, smtp.GetValue("Port", 587))
            {
                EnableSsl = smtp.GetValue("EnableSsl", true),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            client.Credentials = new NetworkCredential(username, password);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Password reset email sent to {Email}.", email);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Password reset email was cancelled for {Email}.", email);
            return false;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Password reset email failed for {Email}.", email);
            return false;
        }
    }
}
