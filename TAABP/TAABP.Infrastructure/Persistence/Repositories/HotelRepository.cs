using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class HotelRepository(ApplicationDbContext context) : IHotelRepository
{
    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public void Add(Hotel entity) => context.Hotels.Add(entity);

    public void Update(Hotel entity) => context.Hotels.Update(entity);

    public void Remove(Hotel entity) => context.Hotels.Remove(entity);
}
