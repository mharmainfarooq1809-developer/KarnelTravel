using KarnelTravel.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace KarnelTravel.Middleware;

public class WebsiteAvailabilityMiddleware
{
    private readonly RequestDelegate _next;

    public WebsiteAvailabilityMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ISettingsService settingsService)
    {
        var path = context.Request.Path;
        if (IsBypassed(path, context.User) || IsAdministrator(context.User))
        {
            await _next(context);
            return;
        }

        var settings = await settingsService.GetAsync(context.RequestAborted);
        if (settings.MaintenanceMode)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = "3600";
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    <title>Maintenance · Karnel Travel Guide</title>
                    <style>
                        :root { color-scheme: light; font-family: Arial, sans-serif; background: #f4f7f3; color: #183629; }
                        body { min-height: 100vh; margin: 0; display: grid; place-items: center; padding: 24px; box-sizing: border-box; }
                        main { width: min(620px, 100%); padding: 48px 36px; border: 1px solid #dce7df; border-radius: 24px; background: #fff; box-shadow: 0 20px 60px rgba(24, 54, 41, .12); text-align: center; }
                        .icon { width: 68px; height: 68px; margin: 0 auto 24px; display: grid; place-items: center; border-radius: 50%; background: #e4f1e8; font-size: 32px; }
                        h1 { margin: 0 0 14px; font-size: clamp(2rem, 5vw, 3rem); }
                        p { margin: 0 auto; max-width: 470px; color: #65756b; line-height: 1.7; }
                        small { display: block; margin-top: 28px; color: #8a978e; }
                    </style>
                </head>
                <body>
                    <main>
                        <div class="icon" aria-hidden="true">🔧</div>
                        <h1>We’ll be back soon</h1>
                        <p>Karnel Travel Guide is temporarily unavailable while we perform scheduled maintenance. Please check back shortly.</p>
                        <small>Thank you for your patience.</small>
                    </main>
                </body>
                </html>
                """);
            return;
        }

        if (!settings.PublicWebsiteVisible)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("The website is currently unavailable.");
            return;
        }

        await _next(context);
    }

    private static bool IsBypassed(PathString path, ClaimsPrincipal user)
    {
        if (path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (user.Identity?.IsAuthenticated == true)
            return false;

        return IsAnonymousEntryPoint(path);
    }

    private static bool IsAnonymousEntryPoint(PathString path) =>
        path == "/"
        || path.Equals(new PathString("/Landing"), StringComparison.OrdinalIgnoreCase)
        || path.Equals(new PathString("/Landing/Index"), StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/Register", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/ForgotPassword", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase);

    private static bool IsAdministrator(ClaimsPrincipal user) =>
        user.IsInRole("Admin");
}
