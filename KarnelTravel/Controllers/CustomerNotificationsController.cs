using System.Security.Claims;
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

[Authorize(Roles = "Customer")]
[Route("Customer/Notifications")]
public class CustomerNotificationsController : Controller
{
    private readonly ApplicationDbContext _db;
    public CustomerNotificationsController(ApplicationDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Challenge();
        return View(await _db.CustomerNotifications.AsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken));
    }

    [HttpPost("{id:int}/Read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Challenge();
        var notification = await _db.CustomerNotifications.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, cancellationToken);
        if (notification is null) return NotFound();
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ReadAll")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReadAll(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId)) return Challenge();
        var notifications = await _db.CustomerNotifications.Where(x => x.CustomerId == customerId && !x.IsRead).ToListAsync(cancellationToken);
        foreach (var notification in notifications) { notification.IsRead = true; notification.ReadAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private bool TryGetCustomerId(out int customerId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out customerId) && User.IsInRole("Customer");
}
