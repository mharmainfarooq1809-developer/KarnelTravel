using KarnelTravel.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KarnelTravel.Services;

public sealed class TicketPdfService : ITicketPdfService
{
    static TicketPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(Booking booking, BookingTicket ticket)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(36);
                page.Header().Column(column =>
                {
                    column.Item().Text("KARNEL TRAVEL GUIDE").FontSize(22).Bold().FontColor(Colors.Green.Darken2);
                    column.Item().Text("Digital booking ticket").FontSize(11).FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text($"Ticket reference: {ticket.TicketReference}").Bold();
                    column.Item().Text($"Booking reference: {booking.ReferenceNumber}");
                    column.Item().Text($"Customer: {booking.CustomerName}");
                    column.Item().Text($"Service: {booking.ServiceName}");
                    column.Item().Text($"Category: {booking.ServiceType}");
                    column.Item().Text($"Travel date: {booking.BookingDate:dd MMM yyyy}");
                    if (booking.ReturnDate.HasValue) column.Item().Text($"Return date: {booking.ReturnDate:dd MMM yyyy}");
                    column.Item().Text($"Guests: {booking.Adults} adults, {booking.Children} children");
                    if (booking.Rooms > 0) column.Item().Text($"Rooms: {booking.Rooms}");
                    if (!string.IsNullOrWhiteSpace(booking.PickupLocation)) column.Item().Text($"Pickup: {booking.PickupLocation}");
                    if (!string.IsNullOrWhiteSpace(booking.DropoffLocation)) column.Item().Text($"Drop-off: {booking.DropoffLocation}");
                    column.Item().Text($"Estimated total: {(booking.EstimatedTotalPKR?.ToString("N2") ?? "Not available")} PKR");
                    column.Item().Text($"Status: {booking.Status}").Bold();
                    column.Item().Image(ticket.QrCodePng).FitWidth();
                    column.Item().Text($"Issued: {ticket.IssuedAt:dd MMM yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Footer().AlignCenter().Text("Please keep this ticket available for your journey.").FontSize(9);
            });
        }).GeneratePdf();
    }
}
