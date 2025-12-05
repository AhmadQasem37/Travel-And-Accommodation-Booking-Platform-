using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;
using TAABP.Domain.Entities;

namespace TAABP.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Hotel aggregate root.
/// </summary>
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Hotel entity);
    void Update(Hotel entity);
    void Remove(Hotel entity);
    Task<PagedResult<SearchHotelDto>> SearchAsync(
        SearchHotelsQuery query,
        CancellationToken cancellationToken = default);
}
