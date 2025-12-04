using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class HotelAmenityConfiguration : IEntityTypeConfiguration<HotelAmenity>
{
    public void Configure(EntityTypeBuilder<HotelAmenity> builder)
    {
        builder.ToTable("HotelAmenities");

        // Composite primary key
        builder.HasKey(ha => new { ha.HotelId, ha.AmenityId });

        // Relationships
        builder.HasOne(ha => ha.Hotel)
            .WithMany(h => h.HotelAmenities)
            .HasForeignKey(ha => ha.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ha => ha.Amenity)
            .WithMany(a => a.HotelAmenities)
            .HasForeignKey(ha => ha.AmenityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
