using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;

namespace TAABP.Application.Features.Hotels.Queries.GetFeaturedDeals;

public sealed record GetFeaturedDealsQuery(
    int Count = 5
) : IRequest<Result<IReadOnlyList<FeaturedDealDto>>>;
