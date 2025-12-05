using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Rooms;

namespace TAABP.Application.Features.Rooms.Queries.GetAvailableRooms;

public sealed record GetAvailableRoomsQuery(
    Guid HotelId,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<AvailableRoomDto>>>;
