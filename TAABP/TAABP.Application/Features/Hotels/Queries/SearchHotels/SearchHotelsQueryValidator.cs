using FluentValidation;

namespace TAABP.Application.Features.Hotels.Queries.SearchHotels;

public sealed class SearchHotelsQueryValidator : AbstractValidator<SearchHotelsQuery>
{
    public SearchHotelsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize must be between 1 and 50");

        RuleFor(x => x.SearchQuery)
            .MaximumLength(200)
            .When(x => x.SearchQuery is not null)
            .WithMessage("SearchQuery must not exceed 200 characters");

        RuleFor(x => x.CheckInDate)
            .Must(date => !date.HasValue || date.Value >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("CheckInDate must be today or in the future");

        RuleFor(x => x)
            .Must(x =>
            {
                var checkIn = x.CheckInDate ?? DateOnly.FromDateTime(DateTime.Today);
                var checkOut = x.CheckOutDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
                return checkOut > checkIn;
            })
            .WithMessage("CheckOutDate must be after CheckInDate");

        RuleFor(x => x.Adults)
            .InclusiveBetween(1, 10)
            .WithMessage("Adults must be between 1 and 10");

        RuleFor(x => x.Children)
            .InclusiveBetween(0, 10)
            .WithMessage("Children must be between 0 and 10");

        RuleFor(x => x.Rooms)
            .InclusiveBetween(1, 10)
            .WithMessage("Rooms must be between 1 and 10");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithMessage("MinPrice cannot be negative");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue)
            .WithMessage("MaxPrice cannot be negative");

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice");

        RuleFor(x => x.StarRatings)
            .Must(ratings => ratings is null || ratings.All(r => r >= 1 && r <= 5))
            .WithMessage("Star ratings must be between 1 and 5");
    }
}
