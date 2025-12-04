using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Hotel aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Hotel entity);
    void Update(Hotel entity);
    void Remove(Hotel entity);
}
