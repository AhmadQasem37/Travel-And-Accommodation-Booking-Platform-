using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Booking aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Booking entity);
    void Update(Booking entity);
    void Remove(Booking entity);
}
