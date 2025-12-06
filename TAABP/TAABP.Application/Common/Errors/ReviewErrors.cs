namespace TAABP.Application.Common.Errors;

public static class ReviewErrors
{
    public static Error NotFound(Guid id) =>
        new("Review.NotFound", $"Review with ID '{id}' was not found.");

    public static Error AlreadyReviewed(Guid hotelId) =>
        new("Review.AlreadyReviewed", "You have already reviewed this hotel.");

    public static Error NotOwner =>
        new("Review.NotOwner", "You can only modify your own reviews.");
}
