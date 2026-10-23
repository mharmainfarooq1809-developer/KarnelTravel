using KarnelTravel.Data;
using KarnelTravelGuide.Models;
using KarnelTravelGuide.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
//  MVC
// ----------------------------------------------------------------
builder.Services.AddControllersWithViews();

// ----------------------------------------------------------------
//  EF Core — SQL Server via "KarnelDb" connection string
// ----------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KarnelDb")));

// ----------------------------------------------------------------
//  Admin authentication
//    - PasswordHasher: PBKDF2-HMAC-SHA512 with per-user salt
//    - AuthService:    in-memory seeded users for the design phase.
//                      Swap to EfAdminAuthService once you've created
//                      the AdminUsers table via migrations.
// ----------------------------------------------------------------
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddSingleton<IAdminAuthService, InMemoryAdminAuthService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.Name = "Karnel.Admin.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// ----------------------------------------------------------------
//  HTTP pipeline
// ----------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// IMPORTANT: UseAuthentication must come BEFORE UseAuthorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Conventional route — keeps the public site working (Landing page by default).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}")
    .WithStaticAssets();

// REQUIRED for attribute-routed controllers such as AdminController
// and AccountController — neither will be reachable without this line.
app.MapControllers()
   .WithStaticAssets();

app.Run();