using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;

namespace TAABP.Application.Features.Hotels.Queries.GetRecentlyVisitedHotels;

/// <summary>
/// Query to get recently visited hotels by the authenticated user.
/// </summary>
public sealed record GetRecentlyVisitedHotelsQuery(
    int Count = 5
) : IRequest<Result<IReadOnlyList<RecentlyVisitedHotelDto>>>;
