using System.Security.Cryptography;
using System.Text;
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public sealed class BookingTicketService : IBookingTicketService
{
    private readonly ApplicationDbContext _db;
    private readonly IQrCodeService _qr;
    private readonly ITicketPdfService _pdf;

    public BookingTicketService(ApplicationDbContext db, IQrCodeService qr, ITicketPdfService pdf)
    {
        _db = db;
        _qr = qr;
        _pdf = pdf;
    }

    public async Task<BookingTicket?> SyncForStatusAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.BookingTickets.FirstOrDefaultAsync(x => x.BookingId == booking.Id && x.IsActive, cancellationToken);
        if (booking.Status is "Cancelled" or "Rejected")
        {
            if (ticket is not null)
            {
                ticket.IsActive = false;
                ticket.RevokedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }
            return null;
        }

        if (booking.Status is not "Confirmed" and not "Completed")
            return ticket;

        var qrText = BuildQrText(booking);
        if (ticket is null)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            ticket = new BookingTicket
            {
                BookingId = booking.Id,
                TicketReference = $"TKT-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                VerificationTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))),
                IssuedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _db.BookingTickets.Add(ticket);
        }
        ticket.QrText = qrText;
        ticket.QrCodePng = _qr.GeneratePng(qrText);
        ticket.IsActive = true;
        ticket.RevokedAt = null;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public byte[] GeneratePdf(Booking booking, BookingTicket ticket) => _pdf.Generate(booking, ticket);

    private static string BuildQrText(Booking booking)
    {
        var builder = new StringBuilder();
        builder.AppendLine("KARNEL TRAVEL GUIDE");
        builder.AppendLine("BOOKING CONFIRMATION");
        builder.AppendLine();
        builder.AppendLine($"Booking Reference: {Clean(booking.ReferenceNumber, 40)}");
        builder.AppendLine($"Customer: {Clean(booking.CustomerName, 80)}");
        builder.AppendLine($"Service: {Clean(booking.ServiceName, 100)}");
        builder.AppendLine($"Category: {Clean(booking.ServiceType, 30)}");
        builder.AppendLine($"Travel Date: {booking.BookingDate:dd MMM yyyy}");
        if (booking.ReturnDate.HasValue) builder.AppendLine($"Return Date: {booking.ReturnDate:dd MMM yyyy}");
        builder.AppendLine($"Adults: {booking.Adults}");
        builder.AppendLine($"Children: {booking.Children}");
        if (booking.Rooms > 0) builder.AppendLine($"Rooms: {booking.Rooms}");
        if (booking.EstimatedTotalPKR.HasValue) builder.AppendLine($"Estimated Total: PKR {booking.EstimatedTotalPKR.Value:N0}");
        builder.AppendLine($"Status: {booking.Status}");
        builder.AppendLine();
        builder.AppendLine("Thank you for choosing Karnel Travel Guide.");
        return builder.ToString();
    }

    private static string Clean(string value, int maxLength) => new string(value.Where(c => c is not '\r' and not '\n').Take(maxLength).ToArray()).Trim();
}
