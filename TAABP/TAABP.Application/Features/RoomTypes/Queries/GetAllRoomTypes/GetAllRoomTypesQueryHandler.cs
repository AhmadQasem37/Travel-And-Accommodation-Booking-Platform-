using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.DTOs.RoomTypes;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.RoomTypes.Queries.GetAllRoomTypes;

public sealed class GetAllRoomTypesQueryHandler(
    IRoomTypeRepository roomTypeRepository,
    IMapper mapper,
    ILogger<GetAllRoomTypesQueryHandler> logger)
    : IRequestHandler<GetAllRoomTypesQuery, Result<IReadOnlyList<RoomTypeDto>>>
{
    public async Task<Result<IReadOnlyList<RoomTypeDto>>> Handle(
        GetAllRoomTypesQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all room types");

        var roomTypes = await roomTypeRepository.GetAllAsync(cancellationToken);

        var roomTypeDtos = mapper.Map<IReadOnlyList<RoomTypeDto>>(roomTypes);

        logger.LogInformation("Found {Count} room types", roomTypeDtos.Count);

        return Result<IReadOnlyList<RoomTypeDto>>.Success(roomTypeDtos);
    }
}
