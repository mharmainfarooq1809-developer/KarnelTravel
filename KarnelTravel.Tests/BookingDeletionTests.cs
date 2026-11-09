using KarnelTravel.Models;

namespace KarnelTravel.Tests;

public sealed class BookingDeletionTests
{
    [Theory]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void Completed_or_cancelled_bookings_can_be_deleted_by_admin(string status)
    {
        Assert.True(Booking.CanAdminDelete(status));
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Rejected")]
    public void Other_booking_statuses_cannot_be_deleted_by_admin(string status)
    {
        Assert.False(Booking.CanAdminDelete(status));
    }
}