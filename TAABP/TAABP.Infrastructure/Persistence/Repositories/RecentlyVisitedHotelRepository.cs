using Microsoft.EntityFrameworkCore;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class RecentlyVisitedHotelRepository(ApplicationDbContext context) : IRecentlyVisitedHotelRepository
{
    public async Task<IReadOnlyList<RecentlyVisitedHotelDto>> GetByUserIdAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await context.RecentlyVisitedHotels
            .AsNoTracking()
            .Include(rv => rv.Hotel)
                .ThenInclude(h => h.City)
            .Where(rv => rv.UserId == userId)
            .OrderByDescending(rv => rv.VisitedAt)
            .Take(count)
            .Select(rv => new RecentlyVisitedHotelDto(
                rv.HotelId,
                rv.Hotel.Name,
                rv.Hotel.City.Name,
                rv.Hotel.StarRating,
                rv.Hotel.ThumbnailUrl,
                rv.Hotel.MinRoomPrice,
                rv.VisitedAt))
            .ToListAsync(cancellationToken);
    }

    public void Add(RecentlyVisitedHotel entity) => context.RecentlyVisitedHotels.Add(entity);

    public void Remove(RecentlyVisitedHotel entity) => context.RecentlyVisitedHotels.Remove(entity);
}
