using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

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
}
