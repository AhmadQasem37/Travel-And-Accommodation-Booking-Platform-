using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public void Add(Review entity) => context.Reviews.Add(entity);

    public void Update(Review entity) => context.Reviews.Update(entity);

    public void Remove(Review entity) => context.Reviews.Remove(entity);
}
