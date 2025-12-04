using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class HotelImageConfiguration : IEntityTypeConfiguration<HotelImage>
{
    public void Configure(EntityTypeBuilder<HotelImage> builder)
    {
        builder.ToTable("HotelImages");

        builder.HasKey(hi => hi.Id);

        builder.Property(hi => hi.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
    }
}
