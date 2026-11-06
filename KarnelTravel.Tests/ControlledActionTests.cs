using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Tests;

public sealed class ControlledActionTests
{
    [Fact]
    public async Task Contact_submission_requires_explicit_confirmation()
    {
        await using var db = CreateDatabase();
        var service = new AIControlledActionService(db, new EnabledCapabilities());
        var result = await service.SubmitContactInquiryAsync(new AIInquiryRequest("conversation", "Visitor", "visitor@example.com", "Please contact me.", false), "visitor@example.com");
        Assert.False(result.Success);
        Assert.Contains("confirmation", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.ContactInquiries.ToListAsync());
    }

    [Fact]
    public async Task Contact_submission_persists_after_confirmation()
    {
        await using var db = CreateDatabase();
        var service = new AIControlledActionService(db, new EnabledCapabilities());
        var result = await service.SubmitContactInquiryAsync(new AIInquiryRequest("conversation", "Visitor", "visitor@example.com", "Please contact me.", true), "visitor@example.com");
        Assert.True(result.Success);
        Assert.Single(await db.ContactInquiries.ToListAsync());
        Assert.Single(await db.AIAuditLogs.Where(x => x.Succeeded).ToListAsync());
    }

    [Fact]
    public async Task Booking_status_requires_matching_owner_email()
    {
        await using var db = CreateDatabase();
        db.Bookings.Add(new Booking { ReferenceNumber = "BK-1", CustomerName = "Visitor", Phone = "AI", Email = "visitor@example.com", OwnerEmail = "visitor@example.com", ServiceType = "Hotel", ServiceName = "Stay", BookingDate = DateTime.UtcNow, Status = "Pending" });
        await db.SaveChangesAsync();
        var service = new AIControlledActionService(db, new EnabledCapabilities());
        var denied = await service.GetBookingStatusAsync(new AIBookingStatusRequest("BK-1", "other@example.com", true), "other@example.com");
        var allowed = await service.GetBookingStatusAsync(new AIBookingStatusRequest("BK-1", "visitor@example.com", true), "visitor@example.com");
        Assert.False(denied.Success);
        Assert.True(allowed.Success);
    }

    private static ApplicationDbContext CreateDatabase() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class EnabledCapabilities : IAICapabilityService
    {
        public Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
