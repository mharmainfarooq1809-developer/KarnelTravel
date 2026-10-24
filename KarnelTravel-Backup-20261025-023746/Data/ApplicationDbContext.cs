using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options) { }

	public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

	protected override void OnModelCreating(ModelBuilder model)
	{
		model.Entity<AdminUser>(e =>
		{
			e.ToTable("AdminUsers");
			e.HasKey(x => x.Id);

			// ✅ THIS IS THE MAGIC LINE FOR AUTO-INCREMENT
			e.Property(x => x.Id).ValueGeneratedOnAdd();

			e.Property(x => x.Email).HasMaxLength(256).IsRequired();
			e.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
			e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
			e.Property(x => x.Role).HasMaxLength(64).IsRequired();
			e.HasIndex(x => x.Email).IsUnique();
		});
	}
}