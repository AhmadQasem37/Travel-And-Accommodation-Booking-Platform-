using TAABP.Application.Common;
using TAABP.Application.DTOs.Reviews;
using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Review entity);
    void Update(Review entity);
    void Remove(Review entity);

    Task<PagedResult<ReviewDto>> GetHotelReviewsAsync(
        Guid hotelId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
