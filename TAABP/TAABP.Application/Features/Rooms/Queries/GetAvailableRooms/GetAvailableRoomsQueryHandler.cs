using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Rooms;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.Rooms.Queries.GetAvailableRooms;

public sealed class GetAvailableRoomsQueryHandler(
    IHotelRepository hotelRepository,
    IRoomRepository roomRepository,
    ILogger<GetAvailableRoomsQueryHandler> logger)
    : IRequestHandler<GetAvailableRoomsQuery, Result<PagedResult<AvailableRoomDto>>>
{
    public async Task<Result<PagedResult<AvailableRoomDto>>> Handle(
        GetAvailableRoomsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Getting available rooms for hotel {HotelId}, page {Page}, pageSize {PageSize}",
            request.HotelId, request.Page, request.PageSize);

        var hotel = await hotelRepository.GetByIdAsync(request.HotelId, cancellationToken);

        if (hotel is null)
        {
            logger.LogWarning("Hotel {HotelId} not found", request.HotelId);
            return Result.Failure<PagedResult<AvailableRoomDto>>(HotelErrors.NotFound(request.HotelId));
        }

        var pagedRooms = await roomRepository.GetAvailableRoomsByHotelIdAsync(
            request.HotelId,
            hotel.DiscountPercentage,
            request.Page,
            request.PageSize,
            cancellationToken);

        logger.LogInformation(
            "Found {TotalCount} available rooms for hotel {HotelId}, returning page {Page}",
            pagedRooms.TotalCount,
            request.HotelId,
            request.Page);

        return Result.Success(pagedRooms);
    }
}
