using MediatR;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Reviews;

namespace TAABP.Application.Features.Reviews.Queries.GetHotelReviews;

public sealed record GetHotelReviewsQuery(
    Guid HotelId,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<ReviewDto>>>;
