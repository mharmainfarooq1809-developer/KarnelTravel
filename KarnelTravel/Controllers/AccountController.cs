
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KarnelTravel.Models;
using KarnelTravel.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly IAdminAuthService _auth;
    private readonly ILogger<AccountController> _log;

    public AccountController(
        IAdminAuthService auth,
        ILogger<AccountController> log)
    {
        _auth = auth;
        _log = log;
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password,
        bool rememberMe = false,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Email"] = email;
        ViewData["RememberMe"] = rememberMe;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                string.Empty,
                "Email and password are required.");

            return View();
        }

        var user = await _auth.ValidateCredentialsAsync(email, password);

        if (user is null)
        {
            _log.LogWarning("Failed login attempt for {Email}", email);

            ModelState.AddModelError(
                string.Empty,
                "Those credentials don't match our records.");

            return View();
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
                IsPersistent = rememberMe
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
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        // Normal users go to the public homepage.
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _auth.RegisterUserAsync(
            model.Email,
            model.DisplayName,
            model.Password);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "An account with this email already exists.");

            return View(model);
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

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        // Newly registered normal users go to the homepage.
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet("Denied")]
    public IActionResult Denied()
    {
        return View();
    }
}
