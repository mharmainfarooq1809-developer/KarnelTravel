
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KarnelTravel.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =====================================================
        // DATABASE TABLES
        // =====================================================

        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

        public DbSet<Destination> Destinations => Set<Destination>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<Resort> Resorts => Set<Resort>();

        public DbSet<TourPackage> TourPackages => Set<TourPackage>();
        public DbSet<TourItineraryDay> TourItineraryDays => Set<TourItineraryDay>();
        public DbSet<TourDeparture> TourDepartures => Set<TourDeparture>();

        public DbSet<TransportService> TransportServices
            => Set<TransportService>();

        public DbSet<TransportRoutePricing> TransportRoutePricings
            => Set<TransportRoutePricing>();

        public DbSet<TripInquiry> TripInquiries => Set<TripInquiry>();
        public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Review> Reviews => Set<Review>();

        // =====================================================
        // MODEL CONFIGURATION
        // =====================================================

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            // ADMIN USERS
            model.Entity<AdminUser>(e =>
            {
                e.ToTable("AdminUsers");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
                e.Property(x => x.Role).HasMaxLength(64).IsRequired();

                e.HasIndex(x => x.Email).IsUnique();
            });

            // DESTINATIONS
            model.Entity<Destination>(e =>
            {
                e.ToTable("Destinations");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.Slug).IsUnique();

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                e.Property(x => x.Region).HasMaxLength(100).IsRequired();
                e.Property(x => x.ShortDescription).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
                e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.BestTimeToVisit).HasMaxLength(100).IsRequired();
            });

            // HOTELS
            model.Entity<Hotel>(e =>
            {
                e.ToTable("Hotels");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Slug).IsUnique();

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                e.Property(x => x.City).HasMaxLength(150).IsRequired();
                e.Property(x => x.Address).HasMaxLength(200).IsRequired();
                e.Property(x => x.ShortDescription).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
                e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.ContactPhone).HasMaxLength(100).IsRequired();
                e.Property(x => x.StartingPricePerNight).HasPrecision(18, 2);

                ConfigureDestinationRelationship<Hotel>(e);
            });

            // RESTAURANTS
            model.Entity<Restaurant>(e =>
            {
                e.ToTable("Restaurants");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Slug).IsUnique();

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                e.Property(x => x.City).HasMaxLength(150).IsRequired();
                e.Property(x => x.Address).HasMaxLength(200).IsRequired();
                e.Property(x => x.ShortDescription).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
                e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.CuisineType).HasMaxLength(100).IsRequired();
                e.Property(x => x.ContactPhone).HasMaxLength(100).IsRequired();
                e.Property(x => x.AverageMealPrice).HasPrecision(18, 2);

                ConfigureDestinationRelationship<Restaurant>(e);
            });

            // RESORTS
            model.Entity<Resort>(e =>
            {
                e.ToTable("Resorts");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Slug).IsUnique();

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                e.Property(x => x.CityOrRegion).HasMaxLength(150).IsRequired();
                e.Property(x => x.Address).HasMaxLength(200).IsRequired();
                e.Property(x => x.ShortDescription).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
                e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.ContactPhone).HasMaxLength(100).IsRequired();
                e.Property(x => x.StartingPricePerNight).HasPrecision(18, 2);

                ConfigureDestinationRelationship<Resort>(e);
            });

            // TOUR PACKAGES
            model.Entity<TourPackage>(e =>
            {
                e.ToTable("TourPackages");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.Slug).IsUnique();

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();

                e.Property(x => x.DestinationName).HasMaxLength(150);
                e.Property(x => x.ShortDescription).HasMaxLength(1000);
                e.Property(x => x.Description).HasMaxLength(3000);
                e.Property(x => x.ImageUrl).HasMaxLength(500);
                e.Property(x => x.Inclusions).HasMaxLength(2000);
                e.Property(x => x.Exclusions).HasMaxLength(2000);

                e.Property(x => x.PricePerAdult).HasPrecision(18, 2);
                e.Property(x => x.PricePerChild).HasPrecision(18, 2);

                e.HasOne(x => x.Destination)
                    .WithMany(x => x.TourPackages)
                    .HasForeignKey(x => x.DestinationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TOUR ITINERARY DAYS
            model.Entity<TourItineraryDay>(e =>
            {
                e.ToTable("TourItineraryDays");
                e.HasKey(x => x.Id);

                e.Property(x => x.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                e.Property(x => x.Description)
                    .HasMaxLength(2000);

                e.Property(x => x.OvernightLocation)
                    .HasMaxLength(150);

                e.HasIndex(x => new
                {
                    x.TourPackageId,
                    x.DayNumber
                }).IsUnique();

                e.HasOne(x => x.TourPackage)
                    .WithMany(x => x.ItineraryDays)
                    .HasForeignKey(x => x.TourPackageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TOUR DEPARTURES
            model.Entity<TourDeparture>(e =>
            {
                e.ToTable("TourDepartures", table =>
                {
                    table.HasCheckConstraint(
                        "CK_TourDepartures_DateRange",
                        "[ReturnDate] >= [DepartureDate]");

                    table.HasCheckConstraint(
                        "CK_TourDepartures_SeatCapacity",
                        "[MaximumGroupSize] >= 1 AND " +
                        "[AvailableSeats] >= 0 AND " +
                        "[AvailableSeats] <= [MaximumGroupSize]");
                });

                e.HasKey(x => x.Id);

                e.Property(x => x.MaximumGroupSize).IsRequired();
                e.Property(x => x.AvailableSeats).IsRequired();

                e.HasIndex(x => new
                {
                    x.TourPackageId,
                    x.DepartureDate
                });

                e.HasOne(x => x.TourPackage)
                    .WithMany(x => x.Departures)
                    .HasForeignKey(x => x.TourPackageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TRANSPORT SERVICES
            model.Entity<TransportService>(e =>
            {
                e.ToTable("TransportServices");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                e.Property(x => x.VehicleType).HasMaxLength(100).IsRequired();
                e.Property(x => x.StartingCity).HasMaxLength(150).IsRequired();
                e.Property(x => x.DestinationCity).HasMaxLength(150).IsRequired();
                e.Property(x => x.Description).HasMaxLength(1000).IsRequired();
                e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.ContactPhone).HasMaxLength(100).IsRequired();
                e.Property(x => x.StartingPrice).HasPrecision(18, 2);

                ConfigureDestinationRelationship<TransportService>(e);
            });

            // TRANSPORT ROUTE PRICING
            model.Entity<TransportRoutePricing>(e =>
            {
                e.ToTable("TransportRoutePricings");
                e.HasKey(x => x.Id);

                e.Property(x => x.PickupCity)
                    .HasMaxLength(150)
                    .IsRequired();

                e.Property(x => x.DropoffCity)
                    .HasMaxLength(150)
                    .IsRequired();

                e.Property(x => x.OneWayFarePKR)
                    .HasPrecision(18, 2);

                e.HasIndex(x => new
                {
                    x.TransportServiceId,
                    x.PickupCity,
                    x.DropoffCity
                }).IsUnique();

                e.HasOne(x => x.TransportService)
                    .WithMany()
                    .HasForeignKey(x => x.TransportServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BOOKINGS
            model.Entity<Booking>(e =>
            {
                e.ToTable("Bookings");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.ReferenceNumber).IsUnique();

                e.Property(x => x.ReferenceNumber).HasMaxLength(30).IsRequired();
                e.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(30).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256);
                e.Property(x => x.ServiceType).HasMaxLength(30).IsRequired();
                e.Property(x => x.ServiceName).HasMaxLength(150).IsRequired();

                e.Property(x => x.EstimatedTotalPKR).HasPrecision(18, 2);
                e.Property(x => x.TransportOneWayFarePKR).HasPrecision(18, 2);
                e.Property(x => x.TransportReturnFarePKR).HasPrecision(18, 2);
                e.Property(x => x.TransportDiscountPKR).HasPrecision(18, 2);

                e.Property(x => x.Status).HasMaxLength(30).IsRequired();
                e.Property(x => x.CustomerNotes).HasMaxLength(1000);
                e.Property(x => x.AdminNotes).HasMaxLength(1000);

                e.HasOne<Hotel>()
                    .WithMany()
                    .HasForeignKey(x => x.HotelId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Restaurant>()
                    .WithMany()
                    .HasForeignKey(x => x.RestaurantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Resort>()
                    .WithMany()
                    .HasForeignKey(x => x.ResortId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<TourPackage>()
                    .WithMany()
                    .HasForeignKey(x => x.TourPackageId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TourDeparture)
                    .WithMany(x => x.Bookings)
                    .HasForeignKey(x => x.TourDepartureId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<TransportService>()
                    .WithMany()
                    .HasForeignKey(x => x.TransportServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TRIP INQUIRIES
            model.Entity<TripInquiry>(e =>
            {
                e.ToTable("TripInquiries");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.ReferenceNumber).IsUnique();

                e.Property(x => x.ReferenceNumber).HasMaxLength(30).IsRequired();
                e.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(30).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256);
                e.Property(x => x.Destinations).HasMaxLength(1000).IsRequired();
                e.Property(x => x.ServiceNeeds).HasMaxLength(100).IsRequired();
                e.Property(x => x.BudgetPKR).HasPrecision(18, 2);
                e.Property(x => x.Status).HasMaxLength(30).IsRequired();
            });

            // CONTACT INQUIRIES
            model.Entity<ContactInquiry>(e =>
            {
                e.ToTable("ContactInquiries");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(30);
                e.Property(x => x.Category).HasMaxLength(50).IsRequired();
                e.Property(x => x.Message).HasMaxLength(2000).IsRequired();

                e.Property(x => x.Status)
                    .HasMaxLength(30)
                    .HasDefaultValue("New")
                    .IsRequired();

                e.Property(x => x.AdminNotes).HasMaxLength(2000);

                e.Property(x => x.SubmittedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(x => new { x.Status, x.SubmittedAt });
            });

            // REVIEWS
            model.Entity<Review>(e =>
            {
                e.ToTable("Reviews", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Reviews_ExactlyOneListing",
                        @"(
                            CASE WHEN DestinationId IS NULL THEN 0 ELSE 1 END +
                            CASE WHEN HotelId IS NULL THEN 0 ELSE 1 END +
                            CASE WHEN RestaurantId IS NULL THEN 0 ELSE 1 END +
                            CASE WHEN ResortId IS NULL THEN 0 ELSE 1 END +
                            CASE WHEN TransportServiceId IS NULL THEN 0 ELSE 1 END +
                            CASE WHEN TourPackageId IS NULL THEN 0 ELSE 1 END
                        ) = 1");

                    table.HasCheckConstraint(
                        "CK_Reviews_Rating",
                        "[Rating] >= 1 AND [Rating] <= 5");
                });

                e.HasKey(x => x.Id);

                e.Property(x => x.ReviewerName)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                e.Property(x => x.Comment)
                    .HasMaxLength(2000)
                    .IsRequired();

                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                e.Property(x => x.Status)
                    .HasConversion<int>()
                    .IsRequired();

                e.HasIndex(x => new { x.Status, x.CreatedAt });

                e.HasOne(x => x.Destination)
                    .WithMany()
                    .HasForeignKey(x => x.DestinationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Hotel)
                    .WithMany()
                    .HasForeignKey(x => x.HotelId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Restaurant)
                    .WithMany()
                    .HasForeignKey(x => x.RestaurantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Resort)
                    .WithMany()
                    .HasForeignKey(x => x.ResortId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TransportService)
                    .WithMany()
                    .HasForeignKey(x => x.TransportServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TourPackage)
                    .WithMany()
                    .HasForeignKey(x => x.TourPackageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =====================================================
        // DESTINATION RELATIONSHIP CONFIGURATION
        // =====================================================

        private static void ConfigureDestinationRelationship<TEntity>(
            EntityTypeBuilder<TEntity> e)
            where TEntity : class
        {
            // Existing destination relationship configuration.
        }
    }
}
