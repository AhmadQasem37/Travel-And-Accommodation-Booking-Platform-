using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.DTOs.Reviews;
using TAABP.Application.Interfaces.Repositories;

namespace TAABP.Application.Features.Reviews.Queries.GetHotelReviews;

public sealed class GetHotelReviewsQueryHandler(
    IHotelRepository hotelRepository,
    IReviewRepository reviewRepository,
    ILogger<GetHotelReviewsQueryHandler> logger)
    : IRequestHandler<GetHotelReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    public async Task<Result<PagedResult<ReviewDto>>> Handle(
        GetHotelReviewsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Getting reviews for hotel {HotelId}, page {Page}, pageSize {PageSize}",
            request.HotelId, request.Page, request.PageSize);

        var hotel = await hotelRepository.GetByIdAsync(request.HotelId, cancellationToken);

        if (hotel is null)
        {
            logger.LogWarning("Hotel {HotelId} not found", request.HotelId);
            return Result.Failure<PagedResult<ReviewDto>>(HotelErrors.NotFound(request.HotelId));
        }

        var result = await reviewRepository.GetHotelReviewsAsync(
            request.HotelId,
            request.Page,
            request.PageSize,
            cancellationToken);

        logger.LogInformation(
            "Found {TotalCount} reviews for hotel {HotelId}",
            result.TotalCount, request.HotelId);

        return Result.Success(result);
    }
}
