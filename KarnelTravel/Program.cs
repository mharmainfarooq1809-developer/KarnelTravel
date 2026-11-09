using KarnelTravel.Data;
using KarnelTravel.Middleware;
using KarnelTravel.Models;
using KarnelTravel.Services;
using KarnelTravel.Services.AI;
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
// ----------------------------------------------------------------
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddSingleton<IPasswordHasher<CustomerAccount>, PasswordHasher<CustomerAccount>>();
builder.Services.AddScoped<ICustomerAccountService, CustomerAccountService>();
builder.Services.AddScoped<IBookingStatusService, BookingStatusService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<ITicketPdfService, TicketPdfService>();
builder.Services.AddScoped<IBookingTicketService, BookingTicketService>();
builder.Services.AddScoped<IBookingConfirmationEmailService, BookingConfirmationEmailService>();
builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();

// ✅ CHANGED: Now using the SQL Server database service instead of In-Memory
builder.Services.AddScoped<IAdminAuthService, EfAdminAuthService>();
builder.Services.AddScoped<IWebsiteContentService, WebsiteContentService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ILanguageModelProvider, RuleBasedLanguageProvider>();
builder.Services.AddScoped<IAIEngine, AIEngine>();
builder.Services.AddScoped<ITravelSearchService, TravelSearchService>();
builder.Services.AddScoped<IAIKnowledgeService, AIKnowledgeService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<ITripPlannerService, TripPlannerService>();
builder.Services.AddScoped<IBudgetEstimatorService, BudgetEstimatorService>();
builder.Services.AddScoped<IAIActionService, AIActionService>();
builder.Services.AddScoped<IAIAdministrationService, AIAdministrationService>();
builder.Services.AddScoped<IAICapabilityService>(sp => (IAICapabilityService)sp.GetRequiredService<IAIAdministrationService>());
builder.Services.AddScoped<IAIControlledActionService, AIControlledActionService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.Name = "Karnel.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
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
app.UseMiddleware<WebsiteAvailabilityMiddleware>();
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