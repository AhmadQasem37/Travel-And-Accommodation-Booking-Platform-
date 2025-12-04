using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Amenity aggregate root.
/// Methods will be added as features require them.
/// </summary>
public interface IAmenityRepository
{
    Task<Amenity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Amenity entity);
    void Update(Amenity entity);
    void Remove(Amenity entity);
}
