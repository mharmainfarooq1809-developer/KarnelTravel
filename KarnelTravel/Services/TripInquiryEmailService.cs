using System.Net;
using System.Net.Mail;
using KarnelTravel.Models;

namespace KarnelTravel.Services;

public sealed class TripInquiryEmailService : ITripInquiryEmailService
{
    private readonly IConfiguration _configuration;

    public TripInquiryEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOfferAsync(TripInquiry inquiry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inquiry.Email) || string.IsNullOrWhiteSpace(inquiry.ProposedOffer))
            return;

        var smtp = _configuration.GetSection("Email:Smtp");
        var host = smtp["Host"];
        var from = smtp["From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Trip inquiry email cannot be sent because SMTP is not configured.");

        var subject = $"Your Karnel Travel Guide plan for {inquiry.Destinations}";
        var body = $"Hello {inquiry.CustomerName},\n\n" +
                   $"Our team has reviewed your custom trip request {inquiry.ReferenceNumber}.\n\n" +
                   "Here is how we propose to manage your trip:\n\n" +
                   inquiry.ProposedOffer.Trim() + "\n\n" +
                   $"Destination: {inquiry.Destinations}\n" +
                   $"Travel dates: {inquiry.DepartureDate:dd MMM yyyy} - {inquiry.ReturnDate:dd MMM yyyy}\n\n" +
                   "Please sign in to your Karnel Travel Guide account to review the plan and accept or decline the offer. This is not a confirmed booking until you accept the offer and our team completes confirmation.\n\n" +
                   "Karnel Travel Guide";

        using var message = new MailMessage(from, inquiry.Email.Trim(), subject, body);
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
}
