using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.PricePerNight)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.AdultCapacity)
            .IsRequired();

        builder.Property(r => r.ChildCapacity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasMany(r => r.Images)
            .WithOne(ri => ri.Room)
            .HasForeignKey(ri => ri.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.CartItems)
            .WithOne(ci => ci.Room)
            .HasForeignKey(ci => ci.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.BookingItems)
            .WithOne(bi => bi.Room)
            .HasForeignKey(bi => bi.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
