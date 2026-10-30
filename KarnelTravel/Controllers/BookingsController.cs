
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

    // GET: /Bookings/Create
    [HttpGet("/Bookings/Create")]
    public async Task<IActionResult> Create(string serviceType, int serviceId)
    {
        var service = await FindService(serviceType, serviceId);

        if (service == null)
        {
            return NotFound(
                "The selected service was not found or is not published.");
        }

        var model = new BookingRequestViewModel
        {
            ServiceType = service.Type,
            ServiceId = service.Id,
            BookingDate = DateTime.Today.AddDays(1),
            Adults = 1,
            Children = 0,
            Rooms = 0,
            TripType = service.Type == "Transport" ? "OneWay" : null
        };

        ViewBag.ServiceName = service.Name;

        if (service.Type == "Transport")
        {
            await LoadTransportRoutes(service.Id);
        }

        return View("~/Views/Bookings/Create.cshtml", model);
    }

    // POST: /Bookings/Create
    [HttpPost("/Bookings/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingRequestViewModel model)
    {
        var service = await FindService(
            model.ServiceType,
            model.ServiceId
        );

        if (service == null)
        {
            return NotFound(
                "The selected service was not found or is not published.");
        }

        ViewBag.ServiceName = service.Name;

        decimal? oneWayFare = null;
        decimal? returnFare = null;
        decimal? discount = null;
        decimal? totalFare = null;

        // Validate booking date.
        if (model.BookingDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(model.BookingDate),
                "The booking date cannot be in the past.");
        }

        // Validate return date.
        if (model.ReturnDate.HasValue)
        {
            if (model.ReturnDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.ReturnDate),
                    "The return date cannot be in the past.");
            }

            if (model.ReturnDate.Value.Date < model.BookingDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.ReturnDate),
                    "The return date cannot be before the travel date.");
            }
        }

        // TRANSPORT VALIDATION AND SERVER-SIDE FARE CALCULATION
        if (service.Type == "Transport")
        {
            var pickupCity = model.PickupLocation?.Trim();
            var dropoffCity = model.DropoffLocation?.Trim();

            if (string.IsNullOrWhiteSpace(pickupCity))
            {
                ModelState.AddModelError(
                    nameof(model.PickupLocation),
                    "Please select your pickup city.");
            }

            if (string.IsNullOrWhiteSpace(dropoffCity))
            {
                ModelState.AddModelError(
                    nameof(model.DropoffLocation),
                    "Please select your drop-off city.");
            }

            if (model.TripType != "OneWay" &&
                model.TripType != "RoundTrip")
            {
                ModelState.AddModelError(
                    nameof(model.TripType),
                    "Please select a valid journey type.");
            }

            if (model.TripType == "RoundTrip" &&
                !model.ReturnDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.ReturnDate),
                    "Please select a return date.");
            }

            if (model.TripType == "OneWay")
            {
                model.ReturnDate = null;
            }

            // Check vehicle and passenger capacity.
            var transportService = await _context.TransportServices
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == service.Id && x.IsPublished);

            if (transportService == null)
            {
                return NotFound("The selected vehicle is unavailable.");
            }

            int totalPassengers = model.Adults + model.Children;

            if (model.Adults < 0 || model.Children < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    "Passenger quantities cannot be negative.");
            }

            if (totalPassengers < 1)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    "At least one passenger is required.");
            }

            if (transportService.PassengerCapacity.HasValue &&
                totalPassengers > transportService.PassengerCapacity.Value)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    $"This vehicle can carry a maximum of " +
                    $"{transportService.PassengerCapacity.Value} passengers.");
            }

            // Find active route and calculate fare from database.
            if (!string.IsNullOrWhiteSpace(pickupCity) &&
                !string.IsNullOrWhiteSpace(dropoffCity))
            {
                if (string.Equals(
                    pickupCity,
                    dropoffCity,
                    StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        nameof(model.DropoffLocation),
                        "Pickup and drop-off cities cannot be the same.");
                }
                else
                {
                    var routePricing = await _context.TransportRoutePricings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.TransportServiceId == service.Id &&
                            x.PickupCity == pickupCity &&
                            x.DropoffCity == dropoffCity &&
                            x.IsActive);

                    if (routePricing == null)
                    {
                        ModelState.AddModelError(
                            nameof(model.DropoffLocation),
                            "This route is not currently available for the selected vehicle.");
                    }
                    else
                    {
                        // Outbound fare.
                        oneWayFare = routePricing.OneWayFarePKR;

                        // Return fare:
                        // Use configured return fare when available.
                        // Otherwise, use the outbound fare.
                        if (model.TripType == "RoundTrip")
                        {
                            returnFare =
                                routePricing.ReturnFarePKR
                                ?? routePricing.OneWayFarePKR;
                        }
                        else
                        {
                            returnFare = 0m;
                        }

                        // No discount configured.
                        discount = 0m;

                        // Server-side total.
                        totalFare =
                            oneWayFare.Value +
                            returnFare.Value -
                            discount.Value;
                    }
                }
            }
        }

        // Return form if validation fails.
        if (!ModelState.IsValid)
        {
            if (service.Type == "Transport")
            {
                await LoadTransportRoutes(service.Id);
            }

            return View("~/Views/Bookings/Create.cshtml", model);
        }

        // CREATE BOOKING
        var booking = new Booking
        {
            ReferenceNumber =
                $"KTL-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),

            CustomerName = model.CustomerName.Trim(),

            Phone = model.Phone.Trim(),

            Email = string.IsNullOrWhiteSpace(model.Email)
                ? null
                : model.Email.Trim(),

            ServiceType = service.Type,

            ServiceName = service.Name,

            BookingDate = model.BookingDate.Date,

            ReturnDate = model.ReturnDate?.Date,

            Adults = model.Adults,

            Children = model.Children,

            Rooms = service.Type == "Transport"
                ? 0
                : model.Rooms,

            PickupLocation = service.Type == "Transport"
                ? model.PickupLocation?.Trim()
                : null,

            DropoffLocation = service.Type == "Transport"
                ? model.DropoffLocation?.Trim()
                : null,

            TripType = service.Type == "Transport"
                ? model.TripType
                : null,

            // Save fare snapshots.
            EstimatedTotalPKR = totalFare,

            TransportOneWayFarePKR = oneWayFare,

            TransportReturnFarePKR = returnFare,

            TransportDiscountPKR = discount,

            CustomerNotes = string.IsNullOrWhiteSpace(model.CustomerNotes)
                ? null
                : model.CustomerNotes.Trim(),

            Status = "Pending",

            CreatedAt = DateTime.UtcNow
        };

        // Link booking to selected service.
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
            new
            {
                reference = booking.ReferenceNumber
            }
        );
    }

    // GET: /Bookings/Confirmation
    [HttpGet("/Bookings/Confirmation")]
    public async Task<IActionResult> Confirmation(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.ReferenceNumber == reference
            );

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    // Load active routes for selected vehicle.
    private async Task LoadTransportRoutes(int transportServiceId)
    {
        ViewBag.TransportRoutes = await _context.TransportRoutePricings
            .AsNoTracking()
            .Where(x =>
                x.TransportServiceId == transportServiceId &&
                x.IsActive)
            .OrderBy(x => x.PickupCity)
            .ThenBy(x => x.DropoffCity)
            .ToListAsync();
    }

    // Find selected service from database.
    private async Task<ServiceInfo?> FindService(
        string? serviceType,
        int serviceId)
    {
        if (serviceId <= 0 ||
            string.IsNullOrWhiteSpace(serviceType))
        {
            return null;
        }

        switch (serviceType.Trim().ToLowerInvariant())
        {
            case "destination":
                {
                    var item = await _context.Destinations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId && x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Destination", item.Id, item.Name);
                }

            case "hotel":
                {
                    var item = await _context.Hotels
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId && x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Hotel", item.Id, item.Name);
                }

            case "resort":
                {
                    var item = await _context.Resorts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId && x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Resort", item.Id, item.Name);
                }

            case "restaurant":
                {
                    var item = await _context.Restaurants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId && x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Restaurant", item.Id, item.Name);
                }

            case "tour":
                {
                    var item = await _context.TourPackages
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId && x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Tour", item.Id, item.Name);
                }

            case "transport":
                {
                    var item = await _context.TransportServices
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId && x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Transport", item.Id, item.Name);
                }

            default:
                return null;
        }
    }

    private sealed record ServiceInfo(
        string Type,
        int Id,
        string Name
    );
}
