using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("BookingItems");

        builder.HasKey(bi => bi.Id);

        builder.Property(bi => bi.CheckInDate)
            .IsRequired();

        builder.Property(bi => bi.CheckOutDate)
            .IsRequired();

        builder.Property(bi => bi.PricePerNight)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(bi => bi.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);
    }
}
