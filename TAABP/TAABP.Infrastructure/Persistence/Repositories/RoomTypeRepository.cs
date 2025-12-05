using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class RoomTypeRepository(ApplicationDbContext context) : IRoomTypeRepository
{
    public async Task<IReadOnlyList<RoomType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.RoomTypes
            .AsNoTracking()
            .OrderBy(rt => rt.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.RoomTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public void Add(RoomType entity) => context.RoomTypes.Add(entity);

    public void Update(RoomType entity) => context.RoomTypes.Update(entity);

    public void Remove(RoomType entity) => context.RoomTypes.Remove(entity);
}
