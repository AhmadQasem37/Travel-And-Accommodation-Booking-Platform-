using TAABP.Application.DTOs.Cities;
using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

public interface ICityRepository
{
    Task<City?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(City entity);
    void Update(City entity);
    void Remove(City entity);
    Task<IReadOnlyList<TrendingDestinationDto>> GetTrendingDestinationsAsync(
        int count,
        CancellationToken cancellationToken = default);
}
