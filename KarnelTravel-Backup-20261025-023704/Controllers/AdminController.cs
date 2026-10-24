using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers;

/// <summary>
/// Admin panel controller. Every route requires an authenticated admin.
/// Requests without a valid auth cookie are redirected to /Account/Login
/// with returnUrl set — the AccountController handles the sign-in.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("Admin")]
public class AdminController : Controller
{
    // ============================================================
    //  A. OVERVIEW
    // ============================================================
    [HttpGet("")]
    [HttpGet("Dashboard")]
    public IActionResult Dashboard() => View();

    // ============================================================
    //  B. DESTINATIONS
    // ============================================================
    [HttpGet("Destinations")]
    public IActionResult Destinations() => View();

    [HttpGet("Destinations/Create")]
    public IActionResult CreateDestination()
    {
        ViewData["Title"] = "New destination";
        ViewData["Section"] = "Destinations";
        return View("DestinationEditor");
    }

    [HttpGet("Destinations/Edit/{id?}")]
    public IActionResult EditDestination(string? id)
    {
        ViewData["Title"] = string.IsNullOrEmpty(id) ? "Edit destination" : $"Edit destination #{id}";
        ViewData["Section"] = "Destinations";
        return View("DestinationEditor");
    }

    // ============================================================
    //  D. PROPERTIES  (view not yet built — falls back to ComingSoon)
    // ============================================================
    [HttpGet("Properties")]
    public IActionResult Properties()
    {
        ViewData["Title"] = "Stays & properties";
        ViewData["Section"] = "Stays";
        return View("ComingSoon");
    }

    // ============================================================
    //  E. TOURS
    // ============================================================
    [HttpGet("Tours")]
    public IActionResult Tours()
    {
        ViewData["Title"] = "Experiences";
        ViewData["Section"] = "Experiences";
        return View("ComingSoon");
    }

    [HttpGet("Tours/Create")]
    public IActionResult CreateTour()
    {
        ViewData["Title"] = "New tour package";
        ViewData["Section"] = "Experiences";
        return View("TourEditor");
    }

    [HttpGet("Tours/Edit/{id?}")]
    public IActionResult EditTour(string? id)
    {
        ViewData["Title"] = string.IsNullOrEmpty(id) ? "Edit tour" : $"Edit tour #{id}";
        ViewData["Section"] = "Experiences";
        return View("TourEditor");
    }

    // ============================================================
    //  F. BOOKINGS
    // ============================================================
    [HttpGet("Bookings")]
    public IActionResult Bookings() => View();

    // ============================================================
    //  G. TRANSPORT & CALENDAR  (views not yet built — fall back)
    // ============================================================
    [HttpGet("Transport")]
    public IActionResult Transport()
    {
        ViewData["Title"] = "Transport & scheduling";
        ViewData["Section"] = "Transport";
        return View("ComingSoon");
    }

    [HttpGet("Calendar")]
    public IActionResult Calendar()
    {
        ViewData["Title"] = "Calendar";
        ViewData["Section"] = "Calendar";
        return View("ComingSoon");
    }

    // ============================================================
    //  H. INQUIRIES  (view not yet built — falls back)
    // ============================================================
    [HttpGet("Inquiries")]
    public IActionResult Inquiries()
    {
        ViewData["Title"] = "Inquiries";
        ViewData["Section"] = "Inquiries";
        return View("ComingSoon");
    }

    // ============================================================
    //  I. REVIEWS  (view not yet built — falls back)
    // ============================================================
    [HttpGet("Reviews")]
    public IActionResult Reviews()
    {
        ViewData["Title"] = "Reviews";
        ViewData["Section"] = "Reviews";
        return View("ComingSoon");
    }

    // ============================================================
    //  J. WEBSITE CONTENT  (view not yet built — falls back)
    // ============================================================
    [HttpGet("WebsiteContent")]
    public IActionResult WebsiteContent()
    {
        ViewData["Title"] = "Website content";
        ViewData["Section"] = "Website content";
        return View("ComingSoon");
    }

    // ============================================================
    //  K. REPORTS  (view not yet built — falls back)
    // ============================================================
    [HttpGet("Reports")]
    public IActionResult Reports()
    {
        ViewData["Title"] = "Reports & analytics";
        ViewData["Section"] = "Reports";
        return View("ComingSoon");
    }

    // ============================================================
    //  L. SETTINGS  (view not yet built — falls back)
    // ============================================================
    [HttpGet("Settings")]
    public IActionResult Settings()
    {
        ViewData["Title"] = "Settings";
        ViewData["Section"] = "Settings";
        return View("ComingSoon");
    }
}