using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservae.Models;

namespace Reservae.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Space> Spaces => Set<Space>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>().Property(user => user.Name).HasMaxLength(200);

        builder.Entity<Space>(space =>
        {
            space.ToTable("Spaces", table =>
                table.HasCheckConstraint("CK_Spaces_PricePerSpot", "\"PricePerSpot\" >= 0"));
            space.Property(x => x.Title).IsRequired().HasMaxLength(200);
            space.Property(x => x.Address).IsRequired().HasMaxLength(500);
            space.Property(x => x.Description).IsRequired().HasMaxLength(4000);
            space.Property(x => x.CoverImagePath).HasMaxLength(2048);
            space.Property(x => x.PricePerSpot).HasPrecision(18, 2);
            space.HasOne(x => x.Owner)
                .WithMany(user => user.Spaces)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
