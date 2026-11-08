using System.Security.Claims;
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

[Authorize]
[Route("Ticket")]
public class TicketController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBookingTicketService _tickets;
    public TicketController(ApplicationDbContext db, IBookingTicketService tickets)
    {
        _db = db;
        _tickets = tickets;
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var ticket = await _db.BookingTickets.AsNoTracking().Include(x => x.Booking).FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.RevokedAt == null, cancellationToken);
        if (ticket is null) return NotFound();
        if (User.IsInRole("Customer") || User.IsInRole("User"))
        {
            var ownsTicket = User.IsInRole("Customer")
                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId) && ticket.Booking.CustomerId == customerId
                : string.Equals(ticket.Booking.OwnerEmail, User.FindFirstValue(ClaimTypes.Email), StringComparison.OrdinalIgnoreCase);
            if (!ownsTicket)
                return NotFound();
        }
        return View(ticket);
    }

    [HttpGet("Download/{id:int}")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var ticket = await _db.BookingTickets.AsNoTracking().Include(x => x.Booking).FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.RevokedAt == null, cancellationToken);
        if (ticket is null) return NotFound();
        if (User.IsInRole("Customer") || User.IsInRole("User"))
        {
            var ownsTicket = User.IsInRole("Customer")
                ? int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId) && ticket.Booking.CustomerId == customerId
                : string.Equals(ticket.Booking.OwnerEmail, User.FindFirstValue(ClaimTypes.Email), StringComparison.OrdinalIgnoreCase);
            if (!ownsTicket)
                return NotFound();
        }
        return File(_tickets.GeneratePdf(ticket.Booking, ticket), "application/pdf", $"{ticket.TicketReference}.pdf");
    }
}
