using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservae.Models;

namespace Reservae.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<BookableSlot> BookableSlots => Set<BookableSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

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

        builder.Entity<AvailabilityRule>(rule =>
        {
            rule.ToTable("AvailabilityRules", table =>
                table.HasCheckConstraint(
                    "CK_AvailabilityRules_CustomPricePerSpot",
                    "\"CustomPricePerSpot\" IS NULL OR \"CustomPricePerSpot\" >= 0"));
            rule.Property(x => x.CustomPricePerSpot).HasPrecision(18, 2);

            rule.HasOne(x => x.Space)
                .WithMany(x => x.AvailabilityRules)
                .HasForeignKey(x => x.SpaceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BookableSlot>(slot =>
        {
            slot.ToTable("BookableSlots", table =>
                table.HasCheckConstraint(
                    "CK_BookableSlots_CustomPricePerSpot",
                    "\"CustomPricePerSpot\" IS NULL OR \"CustomPricePerSpot\" >= 0"));
            slot.Property(x => x.CustomPricePerSpot).HasPrecision(18, 2);

            slot.HasOne(x => x.AvailabilityRule)
                .WithMany(x => x.BookableSlots)
                .HasForeignKey(x => x.AvailabilityRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            slot.HasOne(x => x.Space)
                .WithMany(x => x.BookableSlots)
                .HasForeignKey(x => x.SpaceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Booking>(booking =>
        {
            booking.HasOne(x => x.BookableSlot)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.BookableSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            booking.HasOne(x => x.UserBooked)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.UserBookedId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
