using System.Net;
using System.Net.Mail;
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public sealed class BookingConfirmationEmailService : IBookingConfirmationEmailService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingConfirmationEmailService> _logger;

    public BookingConfirmationEmailService(
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger<BookingConfirmationEmailService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(Booking booking, BookingTicket ticket, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(booking.Email))
            return;

        var settings = await _db.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? new ApplicationSettings();
        var smtp = _configuration.GetSection("Email:Smtp");
        var host = smtp["Host"];
        var from = smtp["From"] ?? settings.NotificationEmail;

        var attempt = new EmailDeliveryAttempt
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            RecipientEmail = booking.Email.Trim(),
            Subject = $"{settings.NotificationSubjectPrefix} Booking confirmed - {booking.ReferenceNumber}",
            MessageType = "BookingConfirmed",
            Status = string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from) ? "Skipped" : "Pending",
            AttemptCount = 1,
            LastAttemptAt = DateTime.UtcNow
        };
        _db.EmailDeliveryAttempts.Add(attempt);

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            attempt.ProviderMessage = "SMTP email settings are not configured.";
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Booking confirmation email skipped for {Reference}: SMTP is not configured.", booking.ReferenceNumber);
            return;
        }

        try
        {
            var port = smtp.GetValue("Port", 587);
            var enableSsl = smtp.GetValue("EnableSsl", true);
            var username = smtp["Username"];
            var password = smtp["Password"];
            var ticketUrl = BuildTicketUrl(ticket.Id);
            var qrContentId = $"karnel-qr-{ticket.Id}@karneltravel";
            var htmlBody = $"""
                <!doctype html>
                <html><body style="font-family:Arial,sans-serif;color:#1b2922;line-height:1.6">
                <h2>Your booking has been confirmed</h2>
                <p>Hello {WebUtility.HtmlEncode(booking.CustomerName)},</p>
                <p><strong>Booking reference:</strong> {WebUtility.HtmlEncode(booking.ReferenceNumber)}<br>
                <strong>Service:</strong> {WebUtility.HtmlEncode(booking.ServiceName)}<br>
                <strong>Travel date:</strong> {booking.BookingDate:dd MMM yyyy}<br>
                <strong>Ticket reference:</strong> {WebUtility.HtmlEncode(ticket.TicketReference)}</p>
                <p>Present this QR code when requested:</p>
                <p><img src="cid:{qrContentId}" alt="Booking QR code" width="280" height="280" style="display:block;border:0"></p>
                <p>If the QR image is not visible, open your ticket here:<br>
                <a href="{WebUtility.HtmlEncode(ticketUrl)}">View your QR ticket</a></p>
                <p>Please keep this email available for your journey.</p>
                <p>Karnel Travel Guide</p>
                </body></html>
                """;

            using var message = new MailMessage(from, booking.Email.Trim())
            {
                Subject = attempt.Subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            using var qrStream = new MemoryStream(ticket.QrCodePng, writable: false);
            using var qrAttachment = new Attachment(qrStream, "booking-qr.png", "image/png")
            {
                ContentId = qrContentId,
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64
            };
            qrAttachment.ContentDisposition.Inline = true;
            qrAttachment.ContentDisposition.DispositionType = "inline";
            message.Attachments.Add(qrAttachment);
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            if (!string.IsNullOrWhiteSpace(username))
                client.Credentials = new NetworkCredential(username, password);

            await client.SendMailAsync(message, cancellationToken);
            attempt.Status = "Sent";
            attempt.SentAt = DateTime.UtcNow;
            attempt.ProviderMessage = "SMTP delivery completed.";
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            attempt.Status = "Failed";
            attempt.ProviderMessage = exception.Message[..Math.Min(exception.Message.Length, 2000)];
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogError(exception, "Booking confirmation email failed for {Reference}.", booking.ReferenceNumber);
        }
    }

    private string BuildTicketUrl(int ticketId)
    {
        var baseUrl = _configuration["Email:BaseUrl"]?.TrimEnd('/');
        return string.IsNullOrWhiteSpace(baseUrl)
            ? $"/Ticket/Details/{ticketId}"
            : $"{baseUrl}/Ticket/Details/{ticketId}";
    }
}
