using Microsoft.EntityFrameworkCore;
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
}
