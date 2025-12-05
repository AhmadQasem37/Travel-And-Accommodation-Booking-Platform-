using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Application.Features.Hotels.Queries.GetRecentlyVisitedHotels;

public sealed class GetRecentlyVisitedHotelsQueryHandler(
    IRecentlyVisitedHotelRepository recentlyVisitedHotelRepository,
    ICurrentUserService currentUserService,
    ILogger<GetRecentlyVisitedHotelsQueryHandler> logger)
    : IRequestHandler<GetRecentlyVisitedHotelsQuery, Result<IReadOnlyList<RecentlyVisitedHotelDto>>>
{
    public async Task<Result<IReadOnlyList<RecentlyVisitedHotelDto>>> Handle(
        GetRecentlyVisitedHotelsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            logger.LogWarning("Attempted to get recently visited hotels without authentication");
            return Result.Failure<IReadOnlyList<RecentlyVisitedHotelDto>>(UserErrors.Unauthorized);
        }

        logger.LogInformation(
            "Fetching recently visited hotels for user {UserId} - Count: {Count}",
            userId.Value,
            request.Count);

        var hotels = await recentlyVisitedHotelRepository.GetByUserIdAsync(
            userId.Value,
            request.Count,
            cancellationToken);

        logger.LogInformation(
            "Found {Count} recently visited hotels for user {UserId}",
            hotels.Count,
            userId.Value);

        return Result.Success(hotels);
    }
}
