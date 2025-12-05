using Microsoft.EntityFrameworkCore;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Rooms;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class RoomRepository(ApplicationDbContext context) : IRoomRepository
{
    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public void Add(Room entity) => context.Rooms.Add(entity);

    public void Update(Room entity) => context.Rooms.Update(entity);

    public void Remove(Room entity) => context.Rooms.Remove(entity);

    public async Task<PagedResult<AvailableRoomDto>> GetAvailableRoomsByHotelIdAsync(
        Guid hotelId,
        int? discountPercentage,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Rooms
            .AsNoTracking()
            .Where(r => r.HotelId == hotelId && r.IsAvailable)
            .Include(r => r.RoomType)
            .Include(r => r.Images)
            .OrderBy(r => r.PricePerNight);

        var totalCount = await query.CountAsync(cancellationToken);

        var rooms = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AvailableRoomDto(
                r.Id,
                r.RoomNumber,
                r.RoomTypeId,
                r.RoomType.Name,
                r.RoomType.Description,
                r.PricePerNight,
                discountPercentage > 0
                    ? r.PricePerNight * (1 - discountPercentage!.Value / 100m)
                    : null,
                r.AdultCapacity,
                r.ChildCapacity,
                r.Images.Select(i => new RoomImageDto(i.ImageUrl)).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<AvailableRoomDto>(rooms, page, pageSize, totalCount);
    }
}
