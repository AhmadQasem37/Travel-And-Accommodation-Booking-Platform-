using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class AmenityRepository(ApplicationDbContext context) : IAmenityRepository
{
    public async Task<IReadOnlyList<Amenity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Amenities
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Amenity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Amenities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public void Add(Amenity entity) => context.Amenities.Add(entity);

    public void Update(Amenity entity) => context.Amenities.Update(entity);

    public void Remove(Amenity entity) => context.Amenities.Remove(entity);
}
