using TAABP.Application.DTOs.Hotels;
using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for RecentlyVisitedHotel entity.
/// </summary>
public interface IRecentlyVisitedHotelRepository
{
    Task<IReadOnlyList<RecentlyVisitedHotelDto>> GetByUserIdAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default);
    void Add(RecentlyVisitedHotel entity);
    void Remove(RecentlyVisitedHotel entity);
}
