
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers;

public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    private static readonly string[] RestaurantVisitPeriods =
    {
        "Breakfast",
        "Lunch",
        "Afternoon",
        "Dinner"
    };

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET: /Bookings/Create
    // =========================================================

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
            TripType = service.Type == "Transport" ? "OneWay" : null,
            PreferredVisitTime = null,
            TourBookingType = "Scheduled"
        };

        ViewBag.ServiceName = service.Name;

        if (service.Type == "Transport")
        {
            await LoadTransportRoutes(service.Id);
        }

        if (service.Type == "Tour")
        {
            await LoadTourDepartures(service.Id);
        }

        return View("~/Views/Bookings/Create.cshtml", model);
    }

    // =========================================================
    // POST: /Bookings/Create
    // =========================================================

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

        TourDeparture? selectedTourDeparture = null;

        // =====================================================
        // TOUR VALIDATION
        // =====================================================

        if (service.Type == "Tour")
        {
            model.TourBookingType = model.TourBookingType?.Trim();

            int totalTourPassengers = model.Adults + model.Children;

            if (model.Adults < 1 || model.Children < 0 ||
                totalTourPassengers < 1)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    "At least one adult or child passenger is required.");
            }

            model.Rooms = 0;
            model.PickupLocation = null;
            model.DropoffLocation = null;
            model.TripType = null;
            model.PreferredVisitTime = null;

            if (model.TourBookingType == "Scheduled")
            {
                if (!model.TourDepartureId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(model.TourDepartureId),
                        "Please select a scheduled departure.");
                }
                else
                {
                    selectedTourDeparture =
                        await _context.TourDepartures
                            .AsNoTracking()
                            .FirstOrDefaultAsync(d =>
                                d.Id == model.TourDepartureId.Value &&
                                d.TourPackageId == service.Id &&
                                d.IsActive &&
                                d.DepartureDate.Date >= DateTime.Today);

                    if (selectedTourDeparture == null)
                    {
                        ModelState.AddModelError(
                            nameof(model.TourDepartureId),
                            "This departure is unavailable. Please select another.");
                    }
                    else
                    {
                        model.BookingDate =
                            selectedTourDeparture.DepartureDate.Date;

                        model.ReturnDate =
                            selectedTourDeparture.ReturnDate.Date;

                        if (totalTourPassengers >
                            selectedTourDeparture.AvailableSeats)
                        {
                            ModelState.AddModelError(
                                nameof(model.Adults),
                                $"Only {selectedTourDeparture.AvailableSeats} " +
                                "seats are currently available for this departure.");
                        }
                    }
                }
            }
            else if (model.TourBookingType == "Custom")
            {
                model.TourDepartureId = null;
                model.ReturnDate = model.ReturnDate?.Date;
            }
            else
            {
                ModelState.AddModelError(
                    nameof(model.TourBookingType),
                    "Please select Scheduled or Custom booking.");
            }
        }

        // =====================================================
        // GENERAL BOOKING DATE VALIDATION
        // =====================================================

        if (model.BookingDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(model.BookingDate),
                "The booking date cannot be in the past.");
        }

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

        // =====================================================
        // HOTEL VALIDATION
        // =====================================================

        if (service.Type == "Hotel")
        {
            if (!model.ReturnDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.ReturnDate),
                    "Please select your check-out date.");
            }

            if (model.Rooms < 1)
            {
                ModelState.AddModelError(
                    nameof(model.Rooms),
                    "Please select at least one room.");
            }

            if (model.Adults < 1 || model.Children < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    "Please enter valid guest quantities.");
            }
        }

        // =====================================================
        // RESORT VALIDATION
        // =====================================================

        if (service.Type == "Resort")
        {
            if (!model.ReturnDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.ReturnDate),
                    "Please select your check-out date.");
            }

            if (model.Rooms < 1)
            {
                ModelState.AddModelError(
                    nameof(model.Rooms),
                    "Please select at least one room.");
            }

            if (model.Adults < 1 || model.Children < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    "Please enter valid guest quantities.");
            }
        }

        // =====================================================
        // RESTAURANT VALIDATION
        // =====================================================

        if (service.Type == "Restaurant")
        {
            model.PreferredVisitTime =
                model.PreferredVisitTime?.Trim();

            if (string.IsNullOrWhiteSpace(model.PreferredVisitTime))
            {
                ModelState.AddModelError(
                    nameof(model.PreferredVisitTime),
                    "Please select your preferred visit period.");
            }
            else if (!RestaurantVisitPeriods.Contains(
                model.PreferredVisitTime,
                StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.PreferredVisitTime),
                    "Please select a valid restaurant visit period.");
            }
            else
            {
                model.PreferredVisitTime =
                    RestaurantVisitPeriods.First(
                        x => string.Equals(
                            x,
                            model.PreferredVisitTime,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
            }

            model.ReturnDate = null;
            model.Rooms = 0;
            model.PickupLocation = null;
            model.DropoffLocation = null;
            model.TripType = null;
        }
        else
        {
            model.PreferredVisitTime = null;
        }

        // =====================================================
        // TRANSPORT VALIDATION
        // =====================================================

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

            // CHECK VEHICLE

            var transportService = await _context.TransportServices
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == service.Id &&
                    x.IsPublished);

            if (transportService == null)
            {
                return NotFound(
                    "The selected vehicle is unavailable.");
            }

            // PASSENGER CAPACITY

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

            // VEHICLE AVAILABILITY CHECK

            bool validTravelDates =
                model.BookingDate != default &&
                model.BookingDate.Date >= DateTime.Today &&
                (
                    model.TripType == "OneWay" ||
                    (
                        model.TripType == "RoundTrip" &&
                        model.ReturnDate.HasValue &&
                        model.ReturnDate.Value.Date >= model.BookingDate.Date
                    )
                );

            if (validTravelDates)
            {
                DateTime requestedStartDate =
                    model.BookingDate.Date;

                DateTime requestedEndDate =
                    model.TripType == "RoundTrip"
                        ? model.ReturnDate!.Value.Date
                        : requestedStartDate;

                DateTime dayAfterRequestedEnd =
                    requestedEndDate.AddDays(1);

                bool vehicleAlreadyBooked =
                    await _context.Bookings
                        .AsNoTracking()
                        .AnyAsync(b =>
                            b.TransportServiceId == service.Id &&
                            b.ServiceType == "Transport" &&
                            (
                                b.Status == "Pending" ||
                                b.Status == "Confirmed"
                            ) &&
                            b.BookingDate < dayAfterRequestedEnd &&
                            (b.ReturnDate ?? b.BookingDate)
                                >= requestedStartDate
                        );

                if (vehicleAlreadyBooked)
                {
                    ModelState.AddModelError(
                        nameof(model.BookingDate),
                        "This vehicle is already booked for some or all " +
                        "of the selected dates. Please choose different " +
                        "dates or another vehicle.");
                }
            }

            // ROUTE VALIDATION AND FARE CALCULATION

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
                    var routePricing =
                        await _context.TransportRoutePricings
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
                            "This route is not currently available " +
                            "for the selected vehicle.");
                    }
                    else
                    {
                        oneWayFare = routePricing.OneWayFarePKR;

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

                        discount = 0m;

                        totalFare =
                            oneWayFare.Value +
                            returnFare.Value -
                            discount.Value;
                    }
                }
            }
        }

        // =====================================================
        // HOTEL PRICE CALCULATION
        // =====================================================

        if (service.Type == "Hotel" &&
            model.ReturnDate.HasValue &&
            model.ReturnDate.Value.Date > model.BookingDate.Date &&
            model.Rooms >= 1)
        {
            var hotel = await _context.Hotels
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == service.Id &&
                    x.IsPublished);

            if (hotel?.StartingPricePerNight is > 0)
            {
                int nights = (
                    model.ReturnDate.Value.Date -
                    model.BookingDate.Date
                ).Days;

                totalFare =
                    hotel.StartingPricePerNight.Value *
                    model.Rooms *
                    nights;
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This hotel does not have a valid nightly price configured.");
            }
        }

        // =====================================================
        // RESORT PRICE CALCULATION
        // =====================================================

        if (service.Type == "Resort" &&
            model.ReturnDate.HasValue &&
            model.ReturnDate.Value.Date > model.BookingDate.Date &&
            model.Rooms >= 1)
        {
            var resort = await _context.Resorts
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == service.Id &&
                    x.IsPublished);

            if (resort?.StartingPricePerNight is > 0)
            {
                int nights = (
                    model.ReturnDate.Value.Date -
                    model.BookingDate.Date
                ).Days;

                totalFare =
                    resort.StartingPricePerNight.Value *
                    model.Rooms *
                    nights;
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This resort does not have a valid nightly price configured.");
            }
        }

        // =====================================================
        // RESTAURANT PRICE ESTIMATE
        // =====================================================

        if (service.Type == "Restaurant")
        {
            int totalGuests = model.Adults + model.Children;

            if (totalGuests < 1)
            {
                ModelState.AddModelError(
                    nameof(model.Adults),
                    "At least one guest is required.");
            }
            else
            {
                var restaurant = await _context.Restaurants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == service.Id &&
                        x.IsPublished);

                if (restaurant?.AverageMealPrice is > 0)
                {
                    totalFare =
                        restaurant.AverageMealPrice.Value *
                        totalGuests;
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "This restaurant does not have a valid average meal price configured.");
                }
            }
        }

        // =====================================================
        // TOUR PRICE CALCULATION
        // =====================================================

        if (service.Type == "Tour")
        {
            var tour = await _context.TourPackages
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == service.Id &&
                    x.IsPublished);

            if (tour == null)
            {
                return NotFound(
                    "The selected tour is unavailable.");
            }

            if (!tour.PricePerAdult.HasValue ||
                tour.PricePerAdult.Value <= 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This tour does not have a valid adult price configured.");
            }
            else if (model.Children > 0 &&
                     !tour.PricePerChild.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please configure the child price for this tour before accepting child bookings.");
            }
            else
            {
                decimal adultTotal =
                    tour.PricePerAdult.Value * model.Adults;

                decimal childTotal =
                    (tour.PricePerChild ?? 0m) * model.Children;

                totalFare = adultTotal + childTotal;
            }
        }

        // =====================================================
        // RETURN FORM IF VALIDATION FAILS
        // =====================================================

        if (!ModelState.IsValid)
        {
            if (service.Type == "Transport")
            {
                await LoadTransportRoutes(service.Id);
            }

            if (service.Type == "Tour")
            {
                await LoadTourDepartures(service.Id);
            }

            return View("~/Views/Bookings/Create.cshtml", model);
        }

        // =====================================================
        // CREATE BOOKING
        // =====================================================

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

            PreferredVisitTime = service.Type == "Restaurant"
                ? model.PreferredVisitTime
                : null,

            ReturnDate = model.ReturnDate?.Date,

            Adults = model.Adults,

            Children = model.Children,

            Rooms = service.Type == "Transport" ||
                    service.Type == "Restaurant" ||
                    service.Type == "Tour"
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

        // =====================================================
        // LINK BOOKING TO SELECTED SERVICE
        // =====================================================

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

                booking.TourDepartureId =
                    model.TourBookingType == "Scheduled"
                        ? selectedTourDeparture?.Id
                        : null;

                break;

            case "Transport":
                booking.TransportServiceId = service.Id;
                break;
        }

        // =====================================================
        // SAVE BOOKING
        // =====================================================

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

    // =========================================================
    // GET: /Bookings/Confirmation
    // =========================================================

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

    // =========================================================
    // LOAD ACTIVE TRANSPORT ROUTES
    // =========================================================

    private async Task LoadTransportRoutes(int transportServiceId)
    {
        ViewBag.TransportRoutes =
            await _context.TransportRoutePricings
                .AsNoTracking()
                .Where(x =>
                    x.TransportServiceId == transportServiceId &&
                    x.IsActive)
                .OrderBy(x => x.PickupCity)
                .ThenBy(x => x.DropoffCity)
                .ToListAsync();
    }

    // =========================================================
    // LOAD AVAILABLE TOUR DEPARTURES
    // =========================================================

    private async Task LoadTourDepartures(int tourPackageId)
    {
        ViewBag.TourDepartures = await _context.TourDepartures
            .AsNoTracking()
            .Where(d =>
                d.TourPackageId == tourPackageId &&
                d.IsActive &&
                d.DepartureDate.Date >= DateTime.Today &&
                d.AvailableSeats > 0)
            .OrderBy(d => d.DepartureDate)
            .ToListAsync();
    }

    // =========================================================
    // FIND SELECTED SERVICE
    // =========================================================

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
                            x.Id == serviceId &&
                            x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Destination", item.Id, item.Name);
                }

            case "hotel":
                {
                    var item = await _context.Hotels
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId &&
                            x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Hotel", item.Id, item.Name);
                }

            case "resort":
                {
                    var item = await _context.Resorts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId &&
                            x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Resort", item.Id, item.Name);
                }

            case "restaurant":
                {
                    var item = await _context.Restaurants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId &&
                            x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Restaurant", item.Id, item.Name);
                }

            case "tour":
                {
                    var item = await _context.TourPackages
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId &&
                            x.IsPublished);

                    return item == null
                        ? null
                        : new ServiceInfo("Tour", item.Id, item.Name);
                }

            case "transport":
                {
                    var item = await _context.TransportServices
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.Id == serviceId &&
                            x.IsPublished);

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
