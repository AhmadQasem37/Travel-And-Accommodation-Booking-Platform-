using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Review aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Review entity);
    void Update(Review entity);
    void Remove(Review entity);
}
