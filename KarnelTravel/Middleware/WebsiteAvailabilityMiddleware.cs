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
        if (IsBypassed(path) || IsAdministrator(context.User))
        {
            await _next(context);
            return;
        }

        var settings = await settingsService.GetAsync(context.RequestAborted);
        if (settings.MaintenanceMode)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = "3600";
            await context.Response.WriteAsync("Karnel Travel Guide is temporarily unavailable for maintenance.");
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

    private static bool IsBypassed(PathString path)
    {
        return path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAdministrator(ClaimsPrincipal user) =>
        user.IsInRole("Admin");
}
