using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Cart aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Cart entity);
    void Update(Cart entity);
    void Remove(Cart entity);
}
