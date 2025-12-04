using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Room aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Room entity);
    void Update(Room entity);
    void Remove(Room entity);
}
