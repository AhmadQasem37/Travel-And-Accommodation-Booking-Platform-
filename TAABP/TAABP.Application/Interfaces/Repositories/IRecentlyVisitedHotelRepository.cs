using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for RecentlyVisitedHotel entity.
/// Methods will be added as features require them.
/// </summary>
public interface IRecentlyVisitedHotelRepository
{
    void Add(RecentlyVisitedHotel entity);
    void Remove(RecentlyVisitedHotel entity);
}
