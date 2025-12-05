using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.Hotels.Queries.GetFeaturedDeals;

public sealed class GetFeaturedDealsQueryHandler(
    IHotelRepository hotelRepository,
    ILogger<GetFeaturedDealsQueryHandler> logger)
    : IRequestHandler<GetFeaturedDealsQuery, Result<IReadOnlyList<FeaturedDealDto>>>
{
    public async Task<Result<IReadOnlyList<FeaturedDealDto>>> Handle(
        GetFeaturedDealsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching featured deals - Count: {Count}", request.Count);

        var deals = await hotelRepository.GetFeaturedDealsAsync(request.Count, cancellationToken);

        logger.LogInformation("Found {Count} featured deals", deals.Count);

        return Result.Success(deals);
    }
}
