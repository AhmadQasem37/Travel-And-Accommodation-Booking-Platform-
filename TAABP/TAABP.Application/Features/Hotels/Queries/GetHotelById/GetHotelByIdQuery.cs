using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;

namespace TAABP.Application.Features.Hotels.Queries.GetHotelById;

public sealed record GetHotelByIdQuery(Guid HotelId) : IRequest<Result<HotelDetailsDto>>;
