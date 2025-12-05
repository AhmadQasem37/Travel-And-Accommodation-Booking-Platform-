using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Amenities;

namespace TAABP.Application.Features.Amenities.Queries.GetAllAmenities;

public sealed record GetAllAmenitiesQuery : IRequest<Result<IReadOnlyList<AmenityDto>>>;
