using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for RoomType aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IRoomTypeRepository
{
    Task<IReadOnlyList<RoomType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(RoomType entity);
    void Update(RoomType entity);
    void Remove(RoomType entity);
}
