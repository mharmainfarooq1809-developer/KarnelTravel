using System.Security.Claims;
using KarnelTravel.Controllers;
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Tests;

public sealed class DemoPaymentWorkflowTests
{
    [Fact]
    public async Task Successful_demo_payment_records_paid_transaction_and_awaits_approval()
    {
        await using var db = CreateDatabase();
        var booking = AddBooking(db, 7, "owner@example.com", 12500m);
        var controller = CreateController(db, 7, "owner@example.com");

        var result = await controller.SimulateSuccess(booking.Id, CancellationToken.None);

        var transaction = await db.DemoPaymentTransactions.SingleAsync();
        Assert.Equal("Paid (Demo)", transaction.PaymentStatus);
        Assert.StartsWith("DEMO-TXN-", transaction.TransactionReference);
        Assert.Equal(12500m, transaction.Amount);
        Assert.Equal("Pending Admin Approval", booking.Status);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Failed_demo_payment_does_not_mark_booking_paid()
    {
        await using var db = CreateDatabase();
        var booking = AddBooking(db, 7, "owner@example.com", 12500m);
        var controller = CreateController(db, 7, "owner@example.com");

        await controller.SimulateFailure(booking.Id, CancellationToken.None);

        var transaction = await db.DemoPaymentTransactions.SingleAsync();
        Assert.Equal("Failed (Demo)", transaction.PaymentStatus);
        Assert.Equal("Pending", booking.Status);
        Assert.Equal("Simulated demo payment failure.", transaction.FailureReason);
    }

    [Fact]
    public async Task Duplicate_paid_demo_payment_is_rejected()
    {
        await using var db = CreateDatabase();
        var booking = AddBooking(db, 7, "owner@example.com", 12500m);
        db.DemoPaymentTransactions.Add(new DemoPaymentTransaction
        {
            BookingId = booking.Id,
            CustomerId = 7,
            TransactionReference = "DEMO-TXN-EXISTING",
            PaymentStatus = "Paid (Demo)",
            PaymentMethod = "Demo Card",
            Amount = 12500m,
            Currency = "PKR",
            RefundStatus = "Not Applicable"
        });
        await db.SaveChangesAsync();
        var controller = CreateController(db, 7, "owner@example.com");

        var result = await controller.SimulateSuccess(booking.Id, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Single(await db.DemoPaymentTransactions.ToListAsync());
    }

    [Fact]
    public async Task Customer_cannot_pay_for_another_customers_booking()
    {
        await using var db = CreateDatabase();
        var booking = AddBooking(db, 8, "other@example.com", 12500m);
        var controller = CreateController(db, 7, "owner@example.com");

        var result = await controller.SimulateSuccess(booking.Id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        Assert.Empty(await db.DemoPaymentTransactions.ToListAsync());
    }

    [Fact]
    public async Task Missing_booking_amount_is_rejected()
    {
        await using var db = CreateDatabase();
        var booking = AddBooking(db, 7, "owner@example.com", null);
        var controller = CreateController(db, 7, "owner@example.com");

        var result = await controller.SimulateSuccess(booking.Id, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await db.DemoPaymentTransactions.ToListAsync());
    }

    [Theory]
    [InlineData("Pending", "Pending Admin Approval", true)]
    [InlineData("Pending Admin Approval", "Confirmed", true)]
    [InlineData("Pending Admin Approval", "Rejected", true)]
    [InlineData("Pending", "Completed", false)]
    public void Booking_approval_transitions_are_explicit(string current, string next, bool expected)
    {
        Assert.Equal(expected, Booking.CanTransition(current, next));
    }

    [Fact]
    public void Demo_payment_model_contains_no_sensitive_card_properties()
    {
        var forbidden = new[] { "Card" + "Number", "C" + "VV", "C" + "VC", "Expiry" + "Date" };
        var properties = typeof(DemoPaymentTransaction).GetProperties().Select(x => x.Name);
        Assert.DoesNotContain(properties, name => forbidden.Contains(name, StringComparer.OrdinalIgnoreCase));
    }

    private static ApplicationDbContext CreateDatabase() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Booking AddBooking(ApplicationDbContext db, int customerId, string email, decimal? amount)
    {
        var booking = new Booking
        {
            ReferenceNumber = $"KTL-{Guid.NewGuid():N}"[..12],
            CustomerName = "Demo Customer",
            Phone = "03000000000",
            Email = email,
            OwnerEmail = email,
            CustomerId = customerId,
            ServiceType = "Hotel",
            ServiceName = "Demo Hotel",
            BookingDate = DateTime.UtcNow.Date.AddDays(1),
            EstimatedTotalPKR = amount,
            Status = "Pending"
        };
        db.Bookings.Add(booking);
        db.SaveChanges();
        return booking;
    }

    private static DemoPaymentController CreateController(ApplicationDbContext db, int customerId, string email)
    {
        var controller = new DemoPaymentController(db, new TestBookingStatusService(db));
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "Customer")
        }, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    private sealed class TestBookingStatusService : IBookingStatusService
    {
        private readonly ApplicationDbContext _db;
        public TestBookingStatusService(ApplicationDbContext db) => _db = db;

        public async Task<bool> ChangeStatusAsync(Booking booking, string nextStatus, string? changedByUserId, string? changedByRole, string? reason, CancellationToken cancellationToken = default)
        {
            booking.Status = nextStatus;
            _db.BookingStatusHistory.Add(new BookingStatusHistory
            {
                BookingId = booking.Id,
                PreviousStatus = "Pending",
                NewStatus = nextStatus,
                ChangedByUserId = changedByUserId,
                ChangedByRole = changedByRole,
                Reason = reason
            });
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
