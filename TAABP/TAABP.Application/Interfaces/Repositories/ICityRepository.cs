using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for City aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface ICityRepository
{
    Task<City?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(City entity);
    void Update(City entity);
    void Remove(City entity);
}
