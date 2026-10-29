using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/Bookings/Create")]
    public async Task<IActionResult> Create(
        string serviceType, int serviceId)
    {
        var service = await FindService(serviceType, serviceId);

        if (service == null)
            return NotFound("The selected service was not found or is not published.");

        var model = new BookingRequestViewModel
        {
            ServiceType = serviceType,
            ServiceId = serviceId,
            BookingDate = DateTime.Today.AddDays(1)
        };

        ViewBag.ServiceName = service.Name;
        return View(model);
    }

    [HttpPost("/Bookings/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingRequestViewModel model)
    {
        var service = await FindService(model.ServiceType, model.ServiceId);

        if (service == null)
            return NotFound("The selected service was not found or is not published.");

        ViewBag.ServiceName = service.Name;

        if (model.BookingDate.Date < DateTime.Today)
            ModelState.AddModelError(
                nameof(model.BookingDate),
                "The booking date cannot be in the past.");

        if (model.ReturnDate.HasValue &&
            model.ReturnDate.Value.Date < model.BookingDate.Date)
            ModelState.AddModelError(
                nameof(model.ReturnDate),
                "The return or departure date cannot be before the booking date.");

        if (!ModelState.IsValid)
            return View(model);

        var booking = new Booking
        {
            ReferenceNumber = $"KTL-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            CustomerName = model.CustomerName.Trim(),
            Phone = model.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email)
                ? null : model.Email.Trim(),
            ServiceType = service.Type,
            ServiceName = service.Name,
            BookingDate = model.BookingDate.Date,
            ReturnDate = model.ReturnDate?.Date,
            Adults = model.Adults,
            Children = model.Children,
            Rooms = model.Rooms,
            CustomerNotes = model.CustomerNotes?.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        switch (service.Type)
        {
            case "Hotel":
                booking.HotelId = service.Id;
                break;
            case "Resort":
                booking.ResortId = service.Id;
                break;
            case "Restaurant":
                booking.RestaurantId = service.Id;
                break;
            case "Tour":
                booking.TourPackageId = service.Id;
                break;
            case "Transport":
                booking.TransportServiceId = service.Id;
                break;
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Confirmation),
            new { reference = booking.ReferenceNumber });
    }

    [HttpGet("/Bookings/Confirmation")]
    public async Task<IActionResult> Confirmation(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return NotFound();

        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ReferenceNumber == reference);

        if (booking == null)
            return NotFound();

        return View(booking);
    }

    private async Task<ServiceInfo?> FindService(
        string? serviceType, int serviceId)
    {
        if (serviceId <= 0 || string.IsNullOrWhiteSpace(serviceType))
            return null;

        switch (serviceType.Trim().ToLowerInvariant())
        {
            case "destination":
            {
                var item = await _context.Destinations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == serviceId && x.IsPublished);

                return item == null ? null :
                    new ServiceInfo("Destination", item.Id, item.Name);
            }

            case "hotel":
            {
                var item = await _context.Hotels
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == serviceId && x.IsPublished);

                return item == null ? null :
                    new ServiceInfo("Hotel", item.Id, item.Name);
            }

            case "resort":
            {
                var item = await _context.Resorts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == serviceId && x.IsPublished);

                return item == null ? null :
                    new ServiceInfo("Resort", item.Id, item.Name);
            }

            case "restaurant":
            {
                var item = await _context.Restaurants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == serviceId && x.IsPublished);

                return item == null ? null :
                    new ServiceInfo("Restaurant", item.Id, item.Name);
            }

            case "tour":
            {
                var item = await _context.TourPackages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == serviceId && x.IsPublished);

                return item == null ? null :
                    new ServiceInfo("Tour", item.Id, item.Name);
            }

            case "transport":
            {
                var item = await _context.TransportServices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == serviceId && x.IsPublished);

                return item == null ? null :
                    new ServiceInfo("Transport", item.Id, item.Name);
            }

            default:
                return null;
        }
    }

    private sealed record ServiceInfo(
        string Type, int Id, string Name);
}
