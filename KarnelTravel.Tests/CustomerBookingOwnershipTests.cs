using KarnelTravel.Data;
using KarnelTravel.Controllers;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KarnelTravel.Tests;

public sealed class CustomerBookingOwnershipTests
{
    [Fact]
    public async Task Customer_booking_query_returns_only_authenticated_customer_bookings()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new ApplicationDbContext(options);
        db.Bookings.AddRange(
            new Booking { ReferenceNumber = "A-1", CustomerName = "A", Phone = "1", ServiceType = "Hotel", ServiceName = "Hotel", BookingDate = DateTime.UtcNow, CustomerId = 1 },
            new Booking { ReferenceNumber = "B-1", CustomerName = "B", Phone = "2", ServiceType = "Hotel", ServiceName = "Hotel", BookingDate = DateTime.UtcNow, CustomerId = 2 });
        await db.SaveChangesAsync();

        var customerOneBookings = await db.Bookings
            .Where(booking => booking.CustomerId == 1)
            .ToListAsync();

        var customerTwoBookings = await db.Bookings
            .Where(booking => booking.CustomerId == 2)
            .ToListAsync();

        Assert.Single(customerOneBookings);
        Assert.Equal("A-1", customerOneBookings[0].ReferenceNumber);
        Assert.Single(customerTwoBookings);
        Assert.Equal("B-1", customerTwoBookings[0].ReferenceNumber);
    }

    [Fact]
    public async Task Customer_cannot_open_another_customers_ticket_by_changing_id()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new ApplicationDbContext(options);
        var booking = new Booking
        {
            ReferenceNumber = "B-1",
            CustomerName = "Customer B",
            Phone = "2",
            ServiceType = "Hotel",
            ServiceName = "Hotel",
            BookingDate = DateTime.UtcNow,
            CustomerId = 2
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        db.BookingTickets.Add(new BookingTicket
        {
            BookingId = booking.Id,
            Booking = booking,
            TicketReference = "TICKET-B",
            VerificationTokenHash = "hash",
            QrText = "qr",
            QrCodePng = [1]
        });
        await db.SaveChangesAsync();

        var controller = new TicketController(db, new TestTicketService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "Customer")
                }, "Test"))
            }
        };

        var result = await controller.Details(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private sealed class TestTicketService : IBookingTicketService
    {
        public Task<BookingTicket?> SyncForStatusAsync(Booking booking, CancellationToken cancellationToken = default) =>
            Task.FromResult<BookingTicket?>(null);

        public byte[] GeneratePdf(Booking booking, BookingTicket ticket) => [];
    }
}
