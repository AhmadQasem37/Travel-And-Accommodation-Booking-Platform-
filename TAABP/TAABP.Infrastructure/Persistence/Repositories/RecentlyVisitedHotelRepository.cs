using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class RecentlyVisitedHotelRepository(ApplicationDbContext context) : IRecentlyVisitedHotelRepository
{
    public void Add(RecentlyVisitedHotel entity) => context.RecentlyVisitedHotels.Add(entity);

    public void Remove(RecentlyVisitedHotel entity) => context.RecentlyVisitedHotels.Remove(entity);
}
