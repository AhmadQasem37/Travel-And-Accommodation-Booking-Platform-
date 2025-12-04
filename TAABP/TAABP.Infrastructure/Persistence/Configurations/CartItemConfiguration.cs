using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CheckInDate)
            .IsRequired();

        builder.Property(ci => ci.CheckOutDate)
            .IsRequired();

        builder.Property(ci => ci.PriceAtBooking)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ci => ci.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ci => ci.CreatedAt)
            .IsRequired();
    }
}
