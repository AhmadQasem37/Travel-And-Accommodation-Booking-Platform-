using MediatR;
using Microsoft.Extensions.Logging;
using TAABP.Application.Common;
using TAABP.Application.Common.Errors;
using TAABP.Application.Interfaces;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Application.Interfaces.Services;
using TAABP.Domain.Entities;

namespace TAABP.Application.Features.Reviews.Commands.CreateReview;

public sealed class CreateReviewCommandHandler(
    IHotelRepository hotelRepository,
    IReviewRepository reviewRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ILogger<CreateReviewCommandHandler> logger)
    : IRequestHandler<CreateReviewCommand, Result>
{
    public async Task<Result> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId!.Value;

        logger.LogInformation(
            "User {UserId} creating review for hotel {HotelId}",
            userId, request.HotelId);

        var hotel = await hotelRepository.GetByIdAsync(request.HotelId, cancellationToken);

        if (hotel is null)
        {
            logger.LogWarning("Hotel {HotelId} not found", request.HotelId);
            return Result.Failure(HotelErrors.NotFound(request.HotelId));
        }

        var existingReview = await reviewRepository.GetByUserAndHotelAsync(
            userId, request.HotelId, cancellationToken);

        if (existingReview is not null)
        {
            logger.LogWarning(
                "User {UserId} already reviewed hotel {HotelId}",
                userId, request.HotelId);
            return Result.Failure(ReviewErrors.AlreadyReviewed(request.HotelId));
        }

        var review = new Review
        {
            HotelId = request.HotelId,
            UserId = userId,
            Rating = request.Rating,
            Content = request.Content
        };

        reviewRepository.Add(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "User {UserId} created review {ReviewId} for hotel {HotelId}",
            userId, review.Id, request.HotelId);

        return Result.Success();
    }
}
