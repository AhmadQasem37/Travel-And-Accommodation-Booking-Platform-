using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;

namespace TAABP.Application.Features.Hotels.Queries.GetHotelById;

public sealed class GetHotelByIdQueryHandler(
    IHotelRepository hotelRepository,
    IRecentlyVisitedHotelRepository recentlyVisitedHotelRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ILogger<GetHotelByIdQueryHandler> logger)
    : IRequestHandler<GetHotelByIdQuery, Result<HotelDetailsDto>>
{
    public async Task<Result<HotelDetailsDto>> Handle(
        GetHotelByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting hotel details for ID: {HotelId}", request.HotelId);

        var hotel = await hotelRepository.GetByIdWithDetailsAsync(request.HotelId, cancellationToken);

        if (hotel is null)
        {
            logger.LogWarning("Hotel with ID {HotelId} was not found", request.HotelId);
            return Result.Failure<HotelDetailsDto>(HotelErrors.NotFound(request.HotelId));
        }

        if (currentUserService.UserId.HasValue)
        {
            logger.LogInformation(
                "Recording hotel visit for user {UserId} to hotel {HotelId}",
                currentUserService.UserId.Value,
                request.HotelId);

            await recentlyVisitedHotelRepository.UpsertVisitAsync(
                currentUserService.UserId.Value,
                request.HotelId,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved hotel details for ID: {HotelId}", request.HotelId);

        return Result.Success(hotel);
    }
}
