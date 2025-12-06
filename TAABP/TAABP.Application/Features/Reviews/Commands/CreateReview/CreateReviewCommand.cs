using MediatR;
using TAABP.Application.Common;

namespace TAABP.Application.Features.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(
    Guid HotelId,
    int Rating,
    string Content
) : IRequest<Result>;
