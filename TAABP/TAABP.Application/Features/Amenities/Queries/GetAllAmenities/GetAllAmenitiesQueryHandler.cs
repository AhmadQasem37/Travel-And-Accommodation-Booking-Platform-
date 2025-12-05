using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Amenities;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.Amenities.Queries.GetAllAmenities;

public sealed class GetAllAmenitiesQueryHandler(
    IAmenityRepository amenityRepository,
    IMapper mapper,
    ILogger<GetAllAmenitiesQueryHandler> logger)
    : IRequestHandler<GetAllAmenitiesQuery, Result<IReadOnlyList<AmenityDto>>>
{
    public async Task<Result<IReadOnlyList<AmenityDto>>> Handle(
        GetAllAmenitiesQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all amenities");

        var amenities = await amenityRepository.GetAllAsync(cancellationToken);

        var amenityDtos = mapper.Map<IReadOnlyList<AmenityDto>>(amenities);

        logger.LogInformation("Found {Count} amenities", amenityDtos.Count);

        return Result<IReadOnlyList<AmenityDto>>.Success(amenityDtos);
    }
}
