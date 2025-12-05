using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.RoomTypes;

namespace TAABP.Application.Features.RoomTypes.Queries.GetAllRoomTypes;

public sealed record GetAllRoomTypesQuery : IRequest<Result<IReadOnlyList<RoomTypeDto>>>;
