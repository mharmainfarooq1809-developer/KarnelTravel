using System.Security.Claims;
using KarnelTravelGuide.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace KarnelTravelGuide.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly IAdminAuthService _auth;
    private readonly ILogger<AccountController> _log;

    public AccountController(IAdminAuthService auth, ILogger<AccountController> log)
    {
        _auth = auth;
        _log = log;
    }

    // ---------- GET /Account/Login ----------
    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return SafeRedirect(returnUrl) ?? RedirectToAction("Dashboard", "Admin");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ---------- POST /Account/Login ----------
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

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return View();
        }

        var user = await _auth.ValidateCredentialsAsync(email, password);
        if (user is null)
        {
            _log.LogWarning("Failed admin login attempt for {Email}", email);
            ModelState.AddModelError(string.Empty, "Those credentials don't match our records.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name,           user.DisplayName),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            });

        _log.LogInformation("Admin {Email} signed in", user.Email);

        return SafeRedirect(returnUrl) ?? RedirectToAction("Dashboard", "Admin");
    }

    // ---------- POST /Account/Logout ----------
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var email = User.Identity?.Name;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _log.LogInformation("Admin {Email} signed out", email);
        return RedirectToAction(nameof(Login));
    }

    // ---------- GET /Account/Denied ----------
    [HttpGet("Denied")]
    public IActionResult Denied() => View();

    private IActionResult? SafeRedirect(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : null;
}