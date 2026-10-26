
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

        public DbSet<Destination> Destinations => Set<Destination>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<Resort> Resorts => Set<Resort>();
        public DbSet<TourPackage> TourPackages => Set<TourPackage>();
        public DbSet<TransportService> TransportServices
            => Set<TransportService>();

        public DbSet<TripInquiry> TripInquiries => Set<TripInquiry>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

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

            model.Entity<TourPackage>(e =>
            {
                e.ToTable("TourPackages");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Slug).IsUnique();

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                e.Property(x => x.DestinationName).HasMaxLength(150).IsRequired();
                e.Property(x => x.ShortDescription).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Description).HasMaxLength(3000).IsRequired();
                e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(x => x.Inclusions).HasMaxLength(2000).IsRequired();
                e.Property(x => x.Exclusions).HasMaxLength(2000).IsRequired();
                e.Property(x => x.PricePerAdult).HasPrecision(18, 2);
                e.Property(x => x.PricePerChild).HasPrecision(18, 2);

                ConfigureDestinationRelationship<TourPackage>(e);
            });

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
        }

        private static void ConfigureDestinationRelationship<TEntity>(
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> e)
            where TEntity : class
        {
            // Relationship configuration is declared explicitly for each
            // listing entity below, because each has its own FK property.
        }
    }
}
