
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KarnelTravel.Models;
using KarnelTravel.Services;
using KarnelTravel.Data;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly IAdminAuthService _auth;
    private readonly ILogger<AccountController> _log;
    private readonly ApplicationDbContext _db;
    private readonly ICustomerAccountService _customers;
    private readonly IBookingStatusService _bookingStatusService;
    private readonly IAccountEmailService _accountEmail;

    public AccountController(
        IAdminAuthService auth,
        ILogger<AccountController> log,
        ApplicationDbContext db,
        ICustomerAccountService customers,
        IBookingStatusService bookingStatusService,
        IAccountEmailService accountEmail)
    {
        _auth = auth;
        _log = log;
        _db = db;
        _customers = customers;
        _bookingStatusService = bookingStatusService;
        _accountEmail = accountEmail;
    }

    [HttpGet("Login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CustomerLoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;
        ViewData["Email"] = model.Email;
        ViewData["RememberMe"] = model.RememberMe;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _auth.ValidateCredentialsAsync(model.Email, model.Password);

        if (user is null)
        {
            var customer = await _customers.ValidateCredentialsAsync(model.Email, model.Password);
            if (customer is null)
            {
                _log.LogWarning("Failed customer login attempt");

                ModelState.AddModelError(string.Empty, "The email address or password is incorrect.");
                return View(model);
            }

            var customerClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new(ClaimTypes.Name, customer.FullName),
                new(ClaimTypes.Email, customer.Email),
                new(ClaimTypes.Role, "Customer")
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(customerClaims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            return RedirectAfterLogin(model.ReturnUrl, "Customer", "Dashboard");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            });

        _log.LogInformation(
            "User {Email} signed in",
            user.Email);

        // Only administrators should enter the admin area.
        if (string.Equals(
            user.Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase))
        {
            return RedirectAfterLogin(model.ReturnUrl, "Admin", "Dashboard");
        }

        if (string.Equals(user.Role, "User", StringComparison.OrdinalIgnoreCase))
            return RedirectAfterLogin(model.ReturnUrl, "Customer", "Dashboard");

        // Other non-administrator users go to the public homepage.
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("Register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await _db.AdminUsers.AnyAsync(x => x.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            return View(model);
        }

        var user = await _customers.CreateAsync(
            model.DisplayName,
            model.Email,
            model.PhoneNumber,
            model.Password);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "An account with this email already exists.");

            return View(model);
        }

        TempData["AccountMessage"] = "Your account was created. You can now sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("ForgotPassword")]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var code = await _customers.CreatePasswordResetCodeAsync(model.Email, cancellationToken);
            if (code is not null)
            {
                var account = await _customers.FindByEmailAsync(model.Email, cancellationToken);
                if (account is not null)
                    await _accountEmail.SendPasswordResetCodeAsync(account, code, cancellationToken);
            }
        }

        TempData["AccountMessage"] = "If an account matches that address, a password reset code will be sent shortly.";
        return RedirectToAction(nameof(ResetPassword));
    }

    [HttpGet("ResetPassword")]
    [AllowAnonymous]
    public IActionResult ResetPassword() => View(new ResetPasswordViewModel());

    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !await _customers.ResetPasswordWithCodeAsync(model.Email, model.Code, model.Password, cancellationToken))
        {
            ModelState.AddModelError(string.Empty, "This reset link is invalid or expired.");
            return View(model);
        }
        TempData["AccountMessage"] = "Your password has been updated. You can now sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Landing");
    }

    [Authorize]
    [HttpGet("Bookings")]
    public async Task<IActionResult> Bookings(CancellationToken cancellationToken)
    {
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ownerEmail))
            return Forbid();

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(x => x.OwnerEmail == ownerEmail)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return View(bookings);
    }

    [Authorize]
    [HttpPost("Bookings/{id:int}/Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int id, CancellationToken cancellationToken)
    {
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var booking = await _db.Bookings.FirstOrDefaultAsync(
            x => x.Id == id && x.OwnerEmail == ownerEmail,
            cancellationToken);

        if (booking == null)
            return NotFound();

        if (!string.Equals(booking.Status, "Pending", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(booking.Status, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            TempData["BookingError"] = "This booking can no longer be cancelled.";
            return RedirectToAction(nameof(Bookings));
        }

        if (!await _bookingStatusService.ChangeStatusAsync(
                booking,
                "Cancelled",
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.FindFirstValue(ClaimTypes.Role),
                "Cancelled by customer",
                cancellationToken))
        {
            TempData["BookingError"] = "This booking could not be cancelled.";
            return RedirectToAction(nameof(Bookings));
        }

        TempData["BookingMessage"] = "Your booking was cancelled.";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpGet("Denied")]
    [AllowAnonymous]
    public IActionResult Denied()
    {
        return View();
    }

    private IActionResult RedirectAfterLogin(string? returnUrl, string controller, string action)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction(action, controller)!;
    }
}
