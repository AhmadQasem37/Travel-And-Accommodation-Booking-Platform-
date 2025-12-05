using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Cities;

namespace TAABP.Application.Features.Cities.Queries.GetTrendingDestinations;

public sealed record GetTrendingDestinationsQuery(int Count = 5)
    : IRequest<Result<IReadOnlyList<TrendingDestinationDto>>>;
