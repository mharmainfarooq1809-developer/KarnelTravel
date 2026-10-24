using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Existing admin system
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    // Travel listings
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Resort> Resorts => Set<Resort>();
    public DbSet<TourPackage> TourPackages => Set<TourPackage>();
    public DbSet<TransportService> TransportServices => Set<TransportService>();

    // Customer trip requests
    public DbSet<TripInquiry> TripInquiries => Set<TripInquiry>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // AdminUsers
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

        // Destinations
        model.Entity<Destination>(e =>
        {
            e.ToTable("Destinations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            e.Property(x => x.Region).HasMaxLength(100).IsRequired();
        });

        // Hotels
        model.Entity<Hotel>(e =>
        {
            e.ToTable("Hotels");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            e.Property(x => x.City).HasMaxLength(150).IsRequired();
            e.Property(x => x.StartingPricePerNight).HasPrecision(18, 2);
        });

        // Restaurants
        model.Entity<Restaurant>(e =>
        {
            e.ToTable("Restaurants");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            e.Property(x => x.City).HasMaxLength(150).IsRequired();
            e.Property(x => x.AverageMealPrice).HasPrecision(18, 2);
        });

        // Tour packages
        model.Entity<TourPackage>(e =>
        {
            e.ToTable("TourPackages");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            e.Property(x => x.DestinationName).HasMaxLength(150).IsRequired();
            e.Property(x => x.PricePerAdult).HasPrecision(18, 2);
            e.Property(x => x.PricePerChild).HasPrecision(18, 2);
        });

        // Trip inquiries
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

        model.Entity<Resort>(e =>
        {
            e.ToTable("Resorts");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            e.Property(x => x.CityOrRegion).HasMaxLength(150).IsRequired();
            e.Property(x => x.StartingPricePerNight).HasPrecision(18, 2);
        });

        model.Entity<TransportService>(e =>
        {
            e.ToTable("TransportServices");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            e.Property(x => x.StartingPrice).HasPrecision(18, 2);
        });
    }
}