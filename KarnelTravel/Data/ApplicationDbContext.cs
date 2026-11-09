
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
        public DbSet<Experience> Experiences => Set<Experience>();

        public DbSet<TourPackage> TourPackages => Set<TourPackage>();
        public DbSet<TourItineraryDay> TourItineraryDays => Set<TourItineraryDay>();
        public DbSet<TourDeparture> TourDepartures => Set<TourDeparture>();

        public DbSet<TransportService> TransportServices
            => Set<TransportService>();

        public DbSet<TransportRouteVehicle> TransportRouteVehicles
            => Set<TransportRouteVehicle>();

        public DbSet<TransportRoute> TransportRoutes
            => Set<TransportRoute>();

        public DbSet<TripInquiry> TripInquiries => Set<TripInquiry>();
        public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();
        public DbSet<BookingTicket> BookingTickets => Set<BookingTicket>();
        public DbSet<BookingStatusHistory> BookingStatusHistory => Set<BookingStatusHistory>();
        public DbSet<CustomerNotification> CustomerNotifications => Set<CustomerNotification>();
        public DbSet<EmailDeliveryAttempt> EmailDeliveryAttempts => Set<EmailDeliveryAttempt>();
        public DbSet<Review> Reviews => Set<Review>();

        // WEBSITE CONTENT
        public DbSet<WebsiteContent> WebsiteContents
            => Set<WebsiteContent>();

        public DbSet<ApplicationSettings> ApplicationSettings => Set<ApplicationSettings>();
        public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
        public DbSet<AIConfiguration> AIConfigurations => Set<AIConfiguration>();
        public DbSet<AICapability> AICapabilities => Set<AICapability>();
        public DbSet<AIKnowledgeEntry> AIKnowledgeEntries => Set<AIKnowledgeEntry>();
        public DbSet<AIQuickPrompt> AIQuickPrompts => Set<AIQuickPrompt>();
        public DbSet<AIConversation> AIConversations => Set<AIConversation>();
        public DbSet<AIMessage> AIMessages => Set<AIMessage>();
        public DbSet<AIFeedback> AIFeedback => Set<AIFeedback>();
        public DbSet<AIAuditLog> AIAuditLogs => Set<AIAuditLog>();
        public DbSet<AIRecommendationSettings> AIRecommendationSettings => Set<AIRecommendationSettings>();


        // =====================================================
        // MODEL CONFIGURATION
        // =====================================================

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<ApplicationSettings>(e =>
            {
                e.ToTable("ApplicationSettings");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Id).IsUnique();
            });

            model.Entity<Experience>(e =>
            {
                e.ToTable("Experiences");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Category).HasMaxLength(80).IsRequired();
                e.Property(x => x.Location).HasMaxLength(150).IsRequired();
                e.Property(x => x.Duration).HasMaxLength(80).IsRequired();
                e.Property(x => x.ShortDescription).HasMaxLength(1000);
                e.Property(x => x.Description).HasMaxLength(4000);
                e.Property(x => x.ImageUrl).HasMaxLength(500);
                e.Property(x => x.Includes).HasMaxLength(1000);
                e.Property(x => x.Excludes).HasMaxLength(1000);
                e.Property(x => x.StartingPrice).HasPrecision(18, 2);
                e.HasIndex(x => x.Category);
                e.HasIndex(x => new { x.IsPublished, x.DisplayOrder });
            });

            model.Entity<AIConfiguration>(e => { e.ToTable("AIConfigurations"); e.HasKey(x => x.Id); });
            model.Entity<AICapability>(e => { e.ToTable("AICapabilities"); e.HasKey(x => x.Id); e.HasIndex(x => x.Key).IsUnique(); });
            model.Entity<AIKnowledgeEntry>(e => { e.ToTable("AIKnowledgeEntries"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.Category, x.IsEnabled }); });
            model.Entity<AIQuickPrompt>(e => { e.ToTable("AIQuickPrompts"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.DisplayOrder, x.IsEnabled }); });
            model.Entity<AIConversation>(e => { e.ToTable("AIConversations"); e.HasKey(x => x.Id); e.HasIndex(x => x.ConversationKey).IsUnique(); e.HasIndex(x => x.OwnerEmail); e.Property(x => x.OwnerEmail).HasMaxLength(256); e.Property(x => x.AnonymousOwnerKey).HasMaxLength(128); });
            model.Entity<AIMessage>(e => { e.ToTable("AIMessages"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.AIConversationId, x.CreatedAt }); e.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.AIConversationId).OnDelete(DeleteBehavior.Cascade); });
            model.Entity<AIFeedback>(e => { e.ToTable("AIFeedback"); e.HasKey(x => x.Id); e.HasIndex(x => x.ConversationKey); });
            model.Entity<AIAuditLog>(e => { e.ToTable("AIAuditLogs"); e.HasKey(x => x.Id); e.HasIndex(x => x.CreatedAt); });
            model.Entity<AIRecommendationSettings>(e => { e.ToTable("AIRecommendationSettings"); e.HasKey(x => x.Id); e.Property(x => x.LocationWeight).HasPrecision(5, 2); e.Property(x => x.BudgetWeight).HasPrecision(5, 2); e.Property(x => x.FeaturedWeight).HasPrecision(5, 2); e.Property(x => x.CategoryWeight).HasPrecision(5, 2); });

            model.Entity<TripInquiry>(e =>
            {
                e.HasIndex(x => x.OwnerEmail);
                e.Property(x => x.OwnerEmail).HasMaxLength(256);
                e.Property(x => x.AdminNotes).HasMaxLength(2000);
                e.Property(x => x.ProposedOffer).HasMaxLength(2000);
                e.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
                e.HasIndex(x => x.OfferIdempotencyKey).IsUnique().HasFilter("[OfferIdempotencyKey] IS NOT NULL");
            });

            model.Entity<AdminAuditLog>(e =>
            {
                e.ToTable("AdminAuditLogs");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.CreatedAt);
                e.HasIndex(x => x.Action);
            });


            // =================================================
            // ADMIN USERS
            // =================================================

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


            // =================================================
            // DESTINATIONS
            // =================================================

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
                e.Property(x => x.BestTimeToVisit).HasMaxLength(2000).IsRequired();
            });


            // =================================================
            // HOTELS
            // =================================================

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


            // =================================================
            // RESTAURANTS
            // =================================================

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


            // =================================================
            // RESORTS
            // =================================================

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


            // =================================================
            // TOUR PACKAGES
            // =================================================

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


            // =================================================
            // TOUR ITINERARY DAYS
            // =================================================

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


            // =================================================
            // TOUR DEPARTURES
            // =================================================

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


            // =================================================
            // TRANSPORT SERVICES
            // =================================================

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
                e.HasIndex(x => x.IsArchived);

                ConfigureDestinationRelationship<TransportService>(e);
            });


            // =================================================
            // TRANSPORT ROUTES
            // =================================================

            model.Entity<TransportRoute>(e =>
            {
                e.ToTable("TransportRoutes");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();
                e.Property(x => x.Slug)
                    .HasMaxLength(150)
                    .IsRequired();
                e.Property(x => x.StartingCity)
                    .HasMaxLength(150)
                    .IsRequired();
                e.Property(x => x.DestinationCity)
                    .HasMaxLength(150)
                    .IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000);
                e.Property(x => x.StartingPricePKR)
                    .HasPrecision(18, 2);
                e.Property(x => x.ReturnPricePKR).HasPrecision(18, 2);
                e.Property(x => x.EstimatedDistanceKm).HasPrecision(10, 2);
                e.HasIndex(x => x.Slug).IsUnique();
                e.HasIndex(x => new { x.IsPublished, x.IsArchived, x.DisplayOrder });
            });

            model.Entity<TransportRouteVehicle>(e =>
            {
                e.ToTable("TransportRoutePricings");
                e.HasKey(x => x.Id);
                e.Property(x => x.LegacyPickupCity).HasMaxLength(150);
                e.Property(x => x.LegacyDropoffCity).HasMaxLength(150);
                e.Property(x => x.LegacyOneWayFarePKR).HasPrecision(18, 2);
                e.Property(x => x.LegacyReturnFarePKR).HasPrecision(18, 2);
                e.HasIndex(x => new { x.TransportRouteId, x.TransportServiceId }).IsUnique();
                e.HasOne(x => x.TransportRoute)
                    .WithMany(x => x.VehicleLinks)
                    .HasForeignKey(x => x.TransportRouteId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.TransportService)
                    .WithMany(x => x.RouteLinks)
                    .HasForeignKey(x => x.TransportServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =================================================
            // BOOKINGS
            // =================================================

            model.Entity<Booking>(e =>
            {
                e.ToTable("Bookings");
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.ReferenceNumber).IsUnique();

                e.Property(x => x.ReferenceNumber).HasMaxLength(30).IsRequired();
                e.Property(x => x.IdempotencyKey).HasMaxLength(80);
                e.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
                e.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(30).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256);
                e.Property(x => x.OwnerEmail).HasMaxLength(256);
                e.HasIndex(x => x.OwnerEmail);
                e.HasIndex(x => x.CustomerId);
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

                e.HasOne(x => x.TransportRoute)
                    .WithMany()
                    .HasForeignKey(x => x.TransportRouteId)
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

                e.HasOne(x => x.Customer)
                    .WithMany(x => x.Bookings)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            model.Entity<CustomerAccount>(e =>
            {
                e.ToTable("CustomerAccounts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
                e.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
                e.Property(x => x.PasswordResetTokenHash).HasMaxLength(128);
                e.Property(x => x.PasswordResetCodeHash).HasMaxLength(128);
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.PasswordResetCodeHash);
            });

            model.Entity<BookingTicket>(e =>
            {
                e.ToTable("BookingTickets");
                e.HasKey(x => x.Id);
                e.Property(x => x.TicketReference).HasMaxLength(40).IsRequired();
                e.Property(x => x.VerificationTokenHash).HasMaxLength(128).IsRequired();
                e.Property(x => x.QrText).IsRequired();
                e.Property(x => x.QrCodePng).IsRequired();
                e.HasIndex(x => x.TicketReference).IsUnique();
                e.HasIndex(x => x.VerificationTokenHash).IsUnique();
                e.HasIndex(x => x.BookingId).HasFilter("[IsActive] = 1").IsUnique();
                e.HasOne(x => x.Booking)
                    .WithMany(x => x.Tickets)
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<BookingStatusHistory>(e =>
            {
                e.ToTable("BookingStatusHistory");
                e.HasKey(x => x.Id);
                e.Property(x => x.PreviousStatus).HasMaxLength(30).IsRequired();
                e.Property(x => x.NewStatus).HasMaxLength(30).IsRequired();
                e.Property(x => x.ChangedByUserId).HasMaxLength(128);
                e.Property(x => x.ChangedByRole).HasMaxLength(30);
                e.Property(x => x.Reason).HasMaxLength(500);
                e.HasIndex(x => new { x.BookingId, x.ChangedAt });
                e.HasOne(x => x.Booking)
                    .WithMany(x => x.StatusHistory)
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<CustomerNotification>(e =>
            {
                e.ToTable("CustomerNotifications");
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).HasMaxLength(160).IsRequired();
                e.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                e.Property(x => x.NotificationType).HasMaxLength(50).IsRequired();
                e.HasIndex(x => new { x.CustomerId, x.IsRead, x.CreatedAt });
                e.HasOne(x => x.Customer)
                    .WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Booking)
                    .WithMany(x => x.Notifications)
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            model.Entity<EmailDeliveryAttempt>(e =>
            {
                e.ToTable("EmailDeliveryAttempts");
                e.HasKey(x => x.Id);
                e.Property(x => x.RecipientEmail).HasMaxLength(256).IsRequired();
                e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
                e.Property(x => x.MessageType).HasMaxLength(50).IsRequired();
                e.Property(x => x.Status).HasMaxLength(30).IsRequired();
                e.Property(x => x.ProviderMessage).HasMaxLength(2000);
                e.HasIndex(x => new { x.BookingId, x.MessageType, x.Status });
                e.HasOne(x => x.Booking)
                    .WithMany(x => x.EmailDeliveryAttempts)
                    .HasForeignKey(x => x.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
                e.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            // =================================================
            // TRIP INQUIRIES
            // =================================================

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


            // =================================================
            // CONTACT INQUIRIES
            // =================================================

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


            // =================================================
            // REVIEWS
            // =================================================

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


            // =================================================
            // WEBSITE CONTENT
            // =================================================

            model.Entity<WebsiteContent>(e =>
            {
                e.ToTable("WebsiteContents");
                e.HasKey(x => x.Id);

                e.Property(x => x.SiteName)
                    .HasMaxLength(150)
                    .IsRequired();

                e.Property(x => x.Tagline)
                    .HasMaxLength(300);

                e.Property(x => x.LogoUrl)
                    .HasMaxLength(500);

                e.Property(x => x.HomeHeroHeading)
                    .HasMaxLength(200);

                e.Property(x => x.HomeHeroSubtitle)
                    .HasMaxLength(500);

                e.Property(x => x.HomeHeroImageUrl)
                    .HasMaxLength(500);

                e.Property(x => x.HomeHeroButtonText)
                    .HasMaxLength(100);

                e.Property(x => x.AboutHeading)
                    .HasMaxLength(200);

                e.Property(x => x.AboutDescription)
                    .HasMaxLength(3000);

                e.Property(x => x.Mission)
                    .HasMaxLength(2000);

                e.Property(x => x.Vision)
                    .HasMaxLength(2000);

                e.Property(x => x.ContactAddress)
                    .HasMaxLength(300);

                e.Property(x => x.ContactPhone)
                    .HasMaxLength(100);

                e.Property(x => x.ContactEmail)
                    .HasMaxLength(256);

                e.Property(x => x.FooterDescription)
                    .HasMaxLength(1000);

                e.Property(x => x.FacebookUrl)
                    .HasMaxLength(500);

                e.Property(x => x.InstagramUrl)
                    .HasMaxLength(500);

                e.Property(x => x.YouTubeUrl)
                    .HasMaxLength(500);

                e.Property(x => x.DefaultMetaTitle)
                    .HasMaxLength(200);

                e.Property(x => x.DefaultMetaDescription)
                    .HasMaxLength(500);

                e.Property(x => x.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
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
