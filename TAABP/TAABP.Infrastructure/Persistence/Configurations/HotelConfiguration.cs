using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(2000);

        builder.Property(h => h.StarRating)
            .IsRequired();

        builder.Property(h => h.Address)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Latitude)
            .HasPrecision(9, 6);

        builder.Property(h => h.Longitude)
            .HasPrecision(9, 6);

        builder.Property(h => h.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(h => h.MinRoomPrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(h => h.DiscountPercentage);

        builder.Property(h => h.NearbyAttractions)
            .HasMaxLength(2000);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.HasMany(h => h.Rooms)
            .WithOne(r => r.Hotel)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.Images)
            .WithOne(i => i.Hotel)
            .HasForeignKey(i => i.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Reviews)
            .WithOne(r => r.Hotel)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.RecentlyVisitedHotels)
            .WithOne(rv => rv.Hotel)
            .HasForeignKey(rv => rv.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
