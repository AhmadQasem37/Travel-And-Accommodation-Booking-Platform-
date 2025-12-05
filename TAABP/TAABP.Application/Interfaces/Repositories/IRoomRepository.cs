using TAABP.Application.Common;
using TAABP.Application.DTOs.Rooms;
using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Room entity);
    void Update(Room entity);
    void Remove(Room entity);
    Task<PagedResult<AvailableRoomDto>> GetAvailableRoomsByHotelIdAsync(
        Guid hotelId,
        int? discountPercentage,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
