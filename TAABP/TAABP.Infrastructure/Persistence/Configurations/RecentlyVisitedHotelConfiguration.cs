using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAABP.Domain.Entities;

namespace TAABP.Infrastructure.Persistence.Configurations;

public class RecentlyVisitedHotelConfiguration : IEntityTypeConfiguration<RecentlyVisitedHotel>
{
    public void Configure(EntityTypeBuilder<RecentlyVisitedHotel> builder)
    {
        builder.ToTable("RecentlyVisitedHotels");

        builder.HasKey(rv => rv.Id);

        builder.Property(rv => rv.VisitedAt)
            .IsRequired();
    }
}
