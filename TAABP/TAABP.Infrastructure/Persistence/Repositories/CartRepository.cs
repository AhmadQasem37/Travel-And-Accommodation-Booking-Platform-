using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(ApplicationDbContext context) : ICartRepository
{
    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Add(Cart entity) => context.Carts.Add(entity);

    public void Update(Cart entity) => context.Carts.Update(entity);

    public void Remove(Cart entity) => context.Carts.Remove(entity);
}
