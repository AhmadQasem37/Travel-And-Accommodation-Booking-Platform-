using Microsoft.EntityFrameworkCore;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Reviews;
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

    public async Task<Review?> GetByUserAndHotelAsync(
        Guid userId,
        Guid hotelId,
        CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.HotelId == hotelId, cancellationToken);
    }

    public void Add(Review entity) => context.Reviews.Add(entity);

    public void Update(Review entity) => context.Reviews.Update(entity);

    public void Remove(Review entity) => context.Reviews.Remove(entity);

    public async Task<PagedResult<ReviewDto>> GetHotelReviewsAsync(
        Guid hotelId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reviews
            .AsNoTracking()
            .Where(r => r.HotelId == hotelId)
            .Include(r => r.User);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto(
                r.Id,
                r.UserId,
                $"{r.User.FirstName} {r.User.LastName[0]}.",
                r.Rating,
                r.Content,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReviewDto>(reviews, page, pageSize, totalCount);
    }
}
