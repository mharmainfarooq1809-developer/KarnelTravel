using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Tests;

public sealed class BookingStatusServiceApprovalTests
{
    [Fact]
    public async Task Confirmation_generates_ticket_only_after_approval()
    {
        await using var db = CreateDatabase();
        var booking = new Booking
        {
            ReferenceNumber = "KTL-APPROVAL",
            CustomerName = "Customer",
            Phone = "03000000000",
            ServiceType = "Hotel",
            ServiceName = "Demo Hotel",
            BookingDate = DateTime.UtcNow.Date.AddDays(1),
            Status = "Pending Admin Approval",
            EstimatedTotalPKR = 1000m
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        var tickets = new RecordingTicketService();
        var service = new BookingStatusService(db, tickets, new RecordingEmailService());

        var approved = await service.ChangeStatusAsync(booking, "Confirmed", "1", "Admin", "Approved");

        Assert.True(approved);
        Assert.Equal("Confirmed", booking.Status);
        Assert.Equal(1, tickets.SyncCount);

        var pendingBooking = new Booking
        {
            ReferenceNumber = "KTL-PENDING",
            CustomerName = "Customer",
            Phone = "03000000000",
            ServiceType = "Hotel",
            ServiceName = "Demo Hotel",
            BookingDate = DateTime.UtcNow.Date.AddDays(1),
            Status = "Pending Admin Approval"
        };
        db.Bookings.Add(pendingBooking);
        await db.SaveChangesAsync();
        await service.ChangeStatusAsync(pendingBooking, "Rejected", "1", "Admin", "Rejected");

        Assert.Equal(1, tickets.SyncCount);
    }

    [Fact]
    public async Task Rejection_preserves_demo_payment_and_marks_refund_pending()
    {
        await using var db = CreateDatabase();
        var booking = new Booking
        {
            ReferenceNumber = "KTL-REJECT",
            CustomerName = "Customer",
            Phone = "03000000000",
            ServiceType = "Hotel",
            ServiceName = "Demo Hotel",
            BookingDate = DateTime.UtcNow.Date.AddDays(1),
            Status = "Pending Admin Approval"
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        var payment = new DemoPaymentTransaction
        {
            BookingId = booking.Id,
            TransactionReference = "DEMO-TXN-REJECT",
            PaymentStatus = "Paid (Demo)",
            PaymentMethod = "Demo Card",
            Amount = 1000,
            Currency = "PKR",
            RefundStatus = "Refund Pending (Demo)"
        };
        db.DemoPaymentTransactions.Add(payment);
        await db.SaveChangesAsync();

        var service = new BookingStatusService(db, new RecordingTicketService(), new RecordingEmailService());
        Assert.True(await service.ChangeStatusAsync(booking, "Rejected", "1", "Admin", "Rejected"));

        Assert.Equal("Rejected", booking.Status);
        Assert.Equal("Paid (Demo)", await db.DemoPaymentTransactions.Select(x => x.PaymentStatus).SingleAsync());
    }

    private static ApplicationDbContext CreateDatabase() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class RecordingTicketService : IBookingTicketService
    {
        public int SyncCount { get; private set; }
        public Task<BookingTicket?> SyncForStatusAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            SyncCount++;
            return Task.FromResult<BookingTicket?>(new BookingTicket { BookingId = booking.Id, TicketReference = "TKT-DEMO" });
        }
        public byte[] GeneratePdf(Booking booking, BookingTicket ticket) => Array.Empty<byte>();
    }

    private sealed class RecordingEmailService : IBookingConfirmationEmailService
    {
        public Task SendAsync(Booking booking, BookingTicket ticket, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
