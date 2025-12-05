using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Cities;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.Cities.Queries.GetTrendingDestinations;

public sealed class GetTrendingDestinationsQueryHandler(
    ICityRepository cityRepository,
    ILogger<GetTrendingDestinationsQueryHandler> logger)
    : IRequestHandler<GetTrendingDestinationsQuery, Result<IReadOnlyList<TrendingDestinationDto>>>
{
    public async Task<Result<IReadOnlyList<TrendingDestinationDto>>> Handle(
        GetTrendingDestinationsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top {Count} trending destinations", request.Count);

        var trendingDestinations = await cityRepository.GetTrendingDestinationsAsync(
            request.Count,
            cancellationToken);

        logger.LogInformation("Found {Count} trending destinations", trendingDestinations.Count);

        return Result<IReadOnlyList<TrendingDestinationDto>>.Success(trendingDestinations);
    }
}
