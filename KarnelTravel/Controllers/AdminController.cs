using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers;

[Route("Admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    [HttpGet("Dashboard")]
    public IActionResult Dashboard() => View();

    [HttpGet("Destinations")]
    public IActionResult Destinations() => View();

    [HttpGet("Destinations/Create")]
    public IActionResult CreateDestination() => View("DestinationEditor");

    [HttpGet("Destinations/Edit/{id?}")]
    public IActionResult EditDestination(string? id) => View("DestinationEditor");

    [HttpGet("Properties")]
    public IActionResult Properties() => View();

    [HttpGet("Tours")]
    public IActionResult Tours() => View();

    [HttpGet("Tours/Create")]
    public IActionResult CreateTour() => View("TourEditor");

    [HttpGet("Tours/Edit/{id?}")]
    public IActionResult EditTour(string? id) => View("TourEditor");

    [HttpGet("Bookings")]
    public IActionResult Bookings() => View();

    [HttpGet("Transport")]
    public IActionResult Transport() => View();

    [HttpGet("Calendar")]
    public IActionResult Calendar() => View();

    [HttpGet("Inquiries")]
    public IActionResult Inquiries() => View();

    [HttpGet("Reviews")]
    public IActionResult Reviews() => View();

    [HttpGet("WebsiteContent")]
    public IActionResult WebsiteContent() => View();

    [HttpGet("Reports")]
    public IActionResult Reports() => View();

    [HttpGet("Settings")]
    public IActionResult Settings() => View();
}
