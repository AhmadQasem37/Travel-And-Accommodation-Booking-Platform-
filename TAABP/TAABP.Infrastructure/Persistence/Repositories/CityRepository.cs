using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class CityRepository(ApplicationDbContext context) : ICityRepository
{
    public async Task<City?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Cities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Add(City entity) => context.Cities.Add(entity);

    public void Update(City entity) => context.Cities.Update(entity);

    public void Remove(City entity) => context.Cities.Remove(entity);
}
